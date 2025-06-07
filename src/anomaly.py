from datetime import datetime, timedelta
import httpx
from fastapi import FastAPI
from pydantic import BaseModel
import logging
from fastapi.middleware.cors import CORSMiddleware
import re
import json

# Logging pra debugar e ver tudo que acontece rodando a API
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# URL da sua API C# (.NET) — onde manda os alertas gerados por aqui
C_SHARP_ENDPOINT = "https://disasterlink-api.fly.dev/api/alertasclimaticos"

# Guarda o último alerta enviado por cidade, pra não ficar mandando duplicado
last_alert_by_city: dict[str, dict] = {}

# Pra não carregar os mesmos alertas duas vezes quando inicia o serviço
existing_alerts_loaded = False

# Modelo dos dados que chegam do sensor
class Sensors(BaseModel):
    city: str
    temperature: float
    humidity: float
    windSpeed: float  # agora só usamos windSpeed, não tem mais waterLevel

# Modelo do retorno da rota /anomaly
class AlertOut(BaseModel):
    sent: bool
    reason: str | None = None

app = FastAPI()

# Libera o CORS pra tudo — pode acessar de qualquer lugar
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Função que busca os alertas já existentes na sua API .NET pra não duplicar
async def load_existing_alerts():
    global existing_alerts_loaded, last_alert_by_city

    if existing_alerts_loaded:
        return  # se já carregou uma vez, não precisa de novo

    try:
        logger.info("🔄 Carregando alertas existentes da API C#...")
        async with httpx.AsyncClient(timeout=10) as cli:
            resp = await cli.get(C_SHARP_ENDPOINT)
            resp.raise_for_status()
            alerts = resp.json()

            for alert in alerts:
                cidade = alert.get("cidade")
                if not cidade:
                    continue

                # Pega a data/hora no formato "dd/MM/yyyy HH:mm"
                data_hora = alert.get("dataHora", "")
                if data_hora:
                    match = re.match(r"(\d{2})/(\d{2})/(\d{4}) (\d{2}):(\d{2})", data_hora)
                    if match:
                        day, month, year, hour, minute = map(int, match.groups())
                        alert_time = datetime(year, month, day, hour, minute)

                        # Só considera alertas das últimas 24h
                        if datetime.now() - alert_time < timedelta(hours=24):
                            last_alert_by_city[cidade] = {
                                "tipo": alert.get("tipoAlerta", ""),
                                "time": alert_time
                            }
                            logger.info(f"✅ Registrado alerta existente para {cidade}: {alert.get('tipoAlerta')} em {alert_time}")

            logger.info(f"✅ Carregados {len(last_alert_by_city)} alertas recentes da API C#")
    except Exception as e:
        logger.error(f"❌ Erro ao carregar alertas existentes: {str(e)}")
    finally:
        existing_alerts_loaded = True

# Função pra checar se já tem alerta recente pra cidade antes de mandar outro
async def check_existing_alert(city: str) -> bool:
    try:
        day_ago = datetime.now() - timedelta(hours=24)

        async with httpx.AsyncClient(timeout=10) as cli:
            resp = await cli.get(C_SHARP_ENDPOINT)
            resp.raise_for_status()

            if resp.status_code != 200:
                logger.error(f"Erro ao consultar API: status code {resp.status_code}")
                return False

            try:
                alerts = resp.json()
            except json.JSONDecodeError:
                logger.error(f"Erro ao decodificar resposta JSON: {resp.text}")
                return False

            for alert in alerts:
                if alert.get("cidade", "").lower() != city.lower():
                    continue

                data_hora = alert.get("dataHora", "")
                if not data_hora:
                    continue

                match = re.match(r"(\d{2})/(\d{2})/(\d{4}) (\d{2}):(\d{2})", data_hora)
                if not match:
                    continue

                day, month, year, hour, minute = map(int, match.groups())
                alert_time = datetime(year, month, day, hour, minute)

                if datetime.now() - alert_time < timedelta(hours=24):
                    logger.info(f"✅ Encontrado alerta existente para {city}: {alert.get('tipoAlerta')} em {data_hora}")
                    return True

            logger.info(f"✅ Nenhum alerta recente encontrado para {city}")
            return False

    except Exception as e:
        logger.error(f"❌ Erro ao verificar alertas existentes: {str(e)}")
        # Se der erro, finge que não tem alerta (melhor do que perder alerta importante)
        return False

# Função que decide se deve ou não mandar alerta, baseado nos dados dos sensores
def alert_type(t: float, h: float, w: float) -> list[str]:
    alertas = []

    # Temperatura (primeiro os extremos)
    if t <= 5:
        alertas.append("Muito Frio")
    elif t <= 14:
        alertas.append("Frio")

    if t >= 39:
        alertas.append("Calor Extremo")
    elif t >= 32:
        alertas.append("Muito Quente")

    # Vento (também do mais forte pro menos forte)
    if w >= 22.2:
        alertas.append("Vento Perigoso")
    elif w >= 13.9:
        alertas.append("Vento Forte")

    # Umidade
    if h <= 20:
        alertas.append("Umidade Baixa")

    return alertas

# Endpoint principal da API — recebe os dados do sensor e decide se gera alerta
@app.post("/anomaly", response_model=AlertOut)
async def detect_and_forward(s: Sensors):
    logger.info(f"📥 Recebido: {s.dict()}")
    alertas = alert_type(s.temperature, s.humidity, s.windSpeed)

    if not alertas:
        logger.info("ℹ️ Sem anomalia detectada.")
        return {"sent": False, "reason": "Sem anomalia"}

    # Formata valores pros templates das mensagens
    temp = round(s.temperature, 1)
    hum = round(s.humidity, 1)
    wind = round(s.windSpeed, 1)

    # Se tem mais de um alerta, junta tudo num só tipo
    if len(alertas) == 1:
        tipo = alertas[0]
    else:
        tipo = " + ".join(alertas)

    # Antes de mandar, verifica se já tem alerta pra essa cidade nas últimas 24h
    has_recent_alert = await check_existing_alert(s.city)
    if has_recent_alert:
        logger.info(f"ℹ️ Alerta ignorado: {s.city} já possui alerta nas últimas 24h")
        return {"sent": False, "reason": f"Alerta já enviado para {s.city} nas últimas 24h"}

    # Monta mensagem: se só um alerta, usa template simples; se vários, usa combinação
    if len(alertas) == 1:
        descricao = MESSAGE_TEMPLATES[alertas[0]].format(
            cidade=s.city,
            temperatura=temp,
            umidade=hum,
            vento=wind
        )
    else:
        descricao = generate_combined_message(alertas, s.city, temp, hum, wind)

    now = datetime.utcnow()  # Data/hora pra logs, mas API define a data final

    # Prepara o payload pro POST na API C#
    payload = {
        "cidade": s.city,
        "tipoAlerta": tipo,
        "temperatura": temp,
        "umidade": hum,
        "vento": wind,
        "descricao": descricao,
        # dataHora não vai, a API gera
    }

    logger.debug(f"🔍 Verificando condições para alerta:")
    logger.debug(f"   - Tipos detectados: {alertas}")
    logger.debug(f"   - Tipo combinado: {tipo}")
    logger.debug(f"   - Temperatura: {temp}°C")
    logger.debug(f"   - Umidade: {hum}%")
    logger.debug(f"   - Vento: {wind} m/s")
    logger.debug(f"   - Descrição gerada: {descricao}")
    logger.debug(f"   - Payload completo a ser enviado: {json.dumps(payload, ensure_ascii=False)}")

    logger.info(f"📤 Enviando alerta para API C#: {payload}")

    try:
        async with httpx.AsyncClient(timeout=5) as cli:
            logger.debug(f"🔌 Conectando ao endpoint: {C_SHARP_ENDPOINT}")
            logger.debug(f"🔍 JSON exato enviado: {json.dumps(payload, ensure_ascii=False)}")
            resp = await cli.post(C_SHARP_ENDPOINT, json=payload)
            logger.debug(f"📄 Resposta: Status={resp.status_code}, Headers={resp.headers}, Corpo={resp.text}")

            resp.raise_for_status()
            logger.info(f"✅ Alerta enviado com sucesso! Resposta: {resp.status_code}")
            logger.debug(f"📄 Resposta completa: {resp.text}")
            return {"sent": True, "reason": tipo}
    except Exception as e:
        logger.error(f"❌ Erro ao enviar alerta: {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            logger.error(f"📄 Detalhes do erro: Status={e.response.status_code}, Corpo={e.response.text}")
        return {"sent": False, "reason": f"Erro ao enviar: {str(e)}"}

# Função pra criar mensagens combinadas pra quando tem mais de um alerta na cidade ao mesmo tempo
def generate_combined_message(alertas: list[str], cidade: str, temp: float, hum: float, wind: float) -> str:
    tipo_combinado = " + ".join(sorted(alertas))
    if tipo_combinado in COMBINED_TEMPLATES:
        return COMBINED_TEMPLATES[tipo_combinado].format(
            cidade=cidade,
            temperatura=temp,
            umidade=hum,
            vento=wind
        )

    # Se não achar template pronto, monta mensagem na mão mesmo
    categorias = {
        "temperatura": [],
        "vento": [],
        "umidade": []
    }
    for alerta in alertas:
        if alerta in ["Muito Frio", "Frio", "Muito Quente", "Calor Extremo"]:
            categorias["temperatura"].append(alerta)
        elif alerta in ["Vento Forte", "Vento Perigoso"]:
            categorias["vento"].append(alerta)
        elif alerta == "Umidade Baixa":
            categorias["umidade"].append(alerta)

    partes = []
    icones = {
        "temperatura_frio": "❄️",
        "temperatura_quente": "🔥",
        "vento": "🌪️",
        "umidade": "💧"
    }
    if categorias["temperatura"]:
        icone = icones["temperatura_frio"] if "Frio" in "".join(categorias["temperatura"]) else icones["temperatura_quente"]
        temp_alert = categorias["temperatura"][0]
        partes.append(f"{icone} {temp_alert} ({temp}°C)")
    if categorias["vento"]:
        partes.append(f"{icones['vento']} {categorias['vento'][0]} ({wind} m/s)")
    if categorias["umidade"]:
        partes.append(f"{icones['umidade']} Umidade Baixa ({hum}%)")
    alerta_combinado = " + ".join(partes)
    return f"⚠️ ALERTA COMBINADO em {cidade}: {alerta_combinado}! Tome cuidados especiais para todas estas condições simultaneamente."

# Mensagens padrão pra cada tipo de alerta
MESSAGE_TEMPLATES = {
    "Frio":           "❄️ Atenção, {cidade}! A temperatura está baixa ({temperatura}°C). Proteja-se do frio e mantenha-se aquecido.",
    "Muito Frio":     "🥶 Alerta de frio intenso em {cidade} ({temperatura}°C)! Risco de hipotermia. Busque abrigo e agasalhe-se bem.",
    "Muito Quente":   "☀️ Calor intenso em {cidade} ({temperatura}°C)! Beba bastante água, evite o sol forte e procure locais frescos.",
    "Calor Extremo":  "🔥 Perigo: calor extremo em {cidade} ({temperatura}°C)! Risco elevado à saúde. Evite atividades externas e hidrate-se constantemente.",
    "Vento Forte":    "🌬️ Ventos fortes atingindo {cidade} ({vento} m/s)! Cuidado com objetos soltos e estruturas instáveis.",
    "Vento Perigoso": "🌪️ Alerta de vento perigoso em {cidade} ({vento} m/s)! Danos podem ocorrer. Procure abrigo seguro imediatamente.",
    "Umidade Baixa":  "💧 Ar muito seco em {cidade} (umidade {umidade}%)! Mantenha-se hidratado e proteja as vias respiratórias."
}

# Mensagens prontas pra combinações de alertas (dois ou mais ao mesmo tempo)
COMBINED_TEMPLATES = {
    "Calor Extremo + Umidade Baixa": "🔥💧 ATENÇÃO {cidade}! Condições extremamente perigosas: calor extremo ({temperatura}°C) com umidade muito baixa ({umidade}%)! Risco crítico de desidratação e problemas de saúde. Hidrate-se constantemente e evite exposição ao sol.",
    "Muito Quente + Umidade Baixa": "☀️💧 Alerta em {cidade}: calor intenso ({temperatura}°C) combinado com ar muito seco ({umidade}%)! Beba muita água e evite atividades ao ar livre.",
    "Vento Perigoso + Calor Extremo": "🌪️🔥 PERIGO em {cidade}! Combinação de vento perigoso ({vento} m/s) e calor extremo ({temperatura}°C)! Busque abrigo seguro e mantenha-se hidratado.",
    "Muito Frio + Vento Forte": "❄️🌬️ Condições severas em {cidade}: frio intenso ({temperatura}°C) com ventos fortes ({vento} m/s)! Sensação térmica muito baixa. Busque abrigo e agasalhe-se bem.",
    "Frio + Vento Forte": "❄️🌬️ Atenção {cidade}: frio ({temperatura}°C) com ventos fortes ({vento} m/s)! Sensação térmica reduzida. Agasalhe-se bem ao sair.",
    "Vento Forte + Umidade Baixa": "🌬️💧 Alerta em {cidade}: ventos fortes ({vento} m/s) e umidade baixa ({umidade}%)! Risco aumentado de ressecamento e propagação de incêndios.",
    "Muito Quente + Vento Forte": "☀️🌬️ Condições adversas em {cidade}: calor intenso ({temperatura}°C) com ventos fortes ({vento} m/s)! Evite exposição prolongada ao sol e hidrate-se.",

    # Combinações de 3 alertas
    "Calor Extremo + Umidade Baixa + Vento Forte": "🔥💧🌬️ PERIGO EXTREMO em {cidade}! Tríplice ameaça: calor extremo ({temperatura}°C), umidade muito baixa ({umidade}%) e ventos fortes ({vento} m/s)! Risco grave de desidratação, incêndios e problemas respiratórios. Evite qualquer atividade externa, hidrate-se constantemente e busque locais protegidos.",
    "Muito Quente + Umidade Baixa + Vento Forte": "☀️💧🌬️ ALERTA CRÍTICO em {cidade}! Combinação perigosa: calor intenso ({temperatura}°C), baixa umidade ({umidade}%) e ventos fortes ({vento} m/s)! Risco elevado de desidratação e incêndios. Mantenha-se hidratado e em locais protegidos.",
    "Calor Extremo + Vento Perigoso + Umidade Baixa": "🔥🌪️💧 EMERGÊNCIA CLIMÁTICA em {cidade}! Condições extremamente perigosas: calor extremo ({temperatura}°C), ventos perigosos ({vento} m/s) e umidade crítica ({umidade}%)! Risco máximo à saúde e segurança. Busque abrigo seguro imediatamente e mantenha-se hidratado."
}

# Se rodar esse arquivo direto (python anomaly.py), sobe o servidor local pra teste
if __name__ == "__main__":
    import uvicorn
    print("🚀 Iniciando servidor FastAPI na porta 8080...")
    uvicorn.run("app:app", host="127.0.0.1", port=8080, reload=False)
