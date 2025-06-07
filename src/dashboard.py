import streamlit as st
import paho.mqtt.client as mqtt
import json
import requests
import time
from collections import deque
import pandas as pd
from requests.exceptions import ConnectionError
import os
import base64
from datetime import datetime
import threading

# ──────────────────────────────────────────────────────────────────────────────
# CONFIGURAÇÕES GERAIS

# MAX_HISTORY é o número máximo de pontos de dados que guardamos para os gráficos.
# Se chegar a 31, o mais antigo é descartado. É tipo o histórico do seu navegador.
MAX_HISTORY = 30  # quantos pontos históricos manter nos gráficos de sensores

# Configs do broker MQTT. É pra cá que nosso ESP32 manda os dados.
# Usamos um broker público (HiveMQ) pra facilitar os testes.
BROKER = "broker.hivemq.com"
PORT   = 1883
# O tópico MQTT que a gente "escuta". O "+" é um wildcard, ou seja,
# a gente pega dados de qualquer cidade que publicar em "disasterlink/real/*/sensor".
TOPIC = "disasterlink/real/+/sensor"   # usa wildcard '+' para capturar qualquer cidade

# Chave da API do OpenWeatherMap, pra gente poder comparar os dados do nosso sensor
# com dados de uma fonte "oficial".
CITY_NAME   = "São Paulo,BR"
API_KEY_OWM = "3593e408dfaf865658df8fd5ccdf795d"  # substitua pela sua chave real

# URL da nossa outra API, a que foi feita em .NET/C#.
# Ela guarda o histórico de alertas que já rolaram. O dashboard consulta ela.
C_SHARP_API_URL = "https://disasterlink-api.fly.dev/api/alertasclimaticos"

# ──────────────────────────────────────────────────────────────────────────────
# FUNÇÕES DE ESTILO E LAYOUT

# Essa função lê uma imagem do disco e converte pra base64.
# A gente faz isso pra poder embutir a imagem direto no HTML/CSS, sem precisar que o arquivo esteja acessível por uma URL.
def get_img_as_base64(file):
    with open(file, "rb") as f:
        data = f.read()
    return base64.b64encode(data).decode()

def local_css():
    # Aqui a gente define as cores do projeto pra manter a identidade visual.
    primary_blue = "#0094FF"
    dark_blue = "#0062AB"
    light_blue = "#50B5FF"
    bg_color = "#000000"  # Mudado para preto total
    card_bg = "#161B22"
    text_color = "#F0F6FC"

    # Injetamos um bloco de CSS na página do Streamlit.
    # É assim que a gente customiza a aparência pra não ficar com a cara padrão do Streamlit.
    # Usamos f-strings pra colocar as variáveis de cor que definimos acima.
    st.markdown(f"""
    <style>
        /* Estilo global */
        .stApp {{
            background-color: {bg_color};
            color: {text_color};
        }}
        
        /* Header com gradiente */
        .header-container {{
            background: linear-gradient(90deg, #161B22 0%, #0D1117 100%);
            padding: 1.5rem;
            border-radius: 10px;
            margin-bottom: 1rem;
            display: flex;
            align-items: center;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        
        /* Título principal */
        .main-title {{
            color: white;
            font-size: 2.2rem;
            font-weight: 700;
            margin: 0;
            background: linear-gradient(90deg, {primary_blue}, {light_blue});
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }}
        
        .subtitle {{
            color: #A3B9CC;
            font-size: 1.1rem;
            margin-top: 5px;
        }}
        
        /* Cards animados */
        .sensor-card {{
            background-color: {card_bg};
            border-radius: 10px;
            padding: 1.2rem;
            transition: all 0.3s ease;
            border-left: 4px solid {primary_blue};
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
            margin-bottom: 1.5rem; /* Adicionado padding abaixo dos cards */
        }}
        
        .sensor-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 7px 20px rgba(0, 0, 0, 0.2);
        }}
        
        .sensor-value {{
            font-size: 2.5rem;
            font-weight: 700;
            color: white;
            margin: 0.5rem 0;
        }}
        
        .sensor-label {{
            font-size: 1rem;
            color: #A3B9CC;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        
        /* Alerta */
        .alert-card {{
            background-color: rgba(220, 20, 60, 0.15);
            border-radius: 10px;
            padding: 1rem;
            border-left: 4px solid crimson;
            margin-bottom: 1rem;
            animation: pulse 2s infinite;
        }}
        
        .alert-card-inactive {{
            background-color: {card_bg};
            border-radius: 10px;
            padding: 1rem;
            border-left: 4px solid #2E8B57;
            margin-bottom: 1rem;
        }}
        
        .alert-title {{
            font-size: 1.2rem;
            font-weight: 600;
            margin-bottom: 0.5rem;
            display: flex;
            align-items: center;
        }}
        
        .alert-message {{
            font-size: 0.95rem;
            color: #E0E0E0;
        }}
        
        @keyframes pulse {{
            0% {{ box-shadow: 0 0 0 0 rgba(220, 20, 60, 0.4); }}
            70% {{ box-shadow: 0 0 0 10px rgba(220, 20, 60, 0); }}
            100% {{ box-shadow: 0 0 0 0 rgba(220, 20, 60, 0); }}
        }}
        
        /* Gráficos */
        .chart-container {{
            background-color: {card_bg};
            border-radius: 10px;
            padding: 1.5rem;  /* Aumentado o padding */
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            margin-bottom: 1.5rem;  /* Aumentado o espaço entre os gráficos */
        }}
        
        .chart-title {{
            font-size: 1.1rem;
            font-weight: 600;
            margin-bottom: 1rem;  /* Aumentado o espaço entre título e gráfico */
            color: white;
        }}
        
        /* Tabs dos gráficos com mais espaço */
        .stTabs [data-baseweb="tab-list"] {{
            gap: 8px;  /* Aumentado o espaço entre as abas */
            background-color: {card_bg};
            border-radius: 8px;
            padding: 5px;  /* Adicionado padding nas abas */
            margin-bottom: 15px;  /* Espaço após as abas */
        }}
        
        .stTabs [data-baseweb="tab"] {{
            background-color: transparent;
            color: {text_color};
            padding: 10px 20px;  /* Mais espaço dentro das abas */
        }}
        
        .stTabs [aria-selected="true"] {{
            background-color: {primary_blue} !important;
            color: white !important;
        }}
        
        /* Status bar */
        .status-bar {{
            background-color: {card_bg};
            border-radius: 8px;
            padding: 0.7rem 1rem;
            font-size: 0.85rem;
            color: #A3B9CC;
            display: flex;
            justify-content: space-between;
            margin-top: 2rem;
        }}
        
        /* Divider */
        .divider {{
            height: 1px;
            background: linear-gradient(90deg, rgba(0,0,0,0) 0%, {primary_blue} 50%, rgba(0,0,0,0) 100%);
            margin: 2rem 0;
            opacity: 0.3;
        }}
        
        /* Customizar widgets do Streamlit */
        .stButton button {{
            background-color: {primary_blue};
            color: white;
            border: none;
            border-radius: 5px;
            padding: 0.5rem 1rem;
            font-weight: 600;
        }}
        
        .stButton button:hover {{
            background-color: {dark_blue};
        }}
        
        /* Alterar cores de texto dos widgets */
        .css-10trblm {{
            color: {light_blue} !important;
        }}
        
        /* Adicionar espaço no mapa */
        .map-container {{
            padding: 15px;
            margin-bottom: 20px;
        }}
        
        /* Esconder o rodapé do Streamlit */
        footer {{
            display: none !important;
        }}
        
        /* Esconder o hamburger menu */
        section[data-testid="stSidebar"] {{
            display: none;
        }}
        
        /* Remover containers vazios */
        .element-container:empty {{
            display: none !important;
        }}
    </style>
    """, unsafe_allow_html=True)
    
    # Adicionar JS para animações suaves
    st.markdown("""
    <script>
    document.addEventListener('DOMContentLoaded', (event) => {
        const cards = document.querySelectorAll('.sensor-card');
        cards.forEach(card => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 300);
        });
    });
    </script>
    """, unsafe_allow_html=True)

def create_header():
    """Cria o cabeçalho estilizado com a logo da DisasterLink Systems"""
    # Tenta carregar a imagem da logo e montar o cabeçalho bonitão.
    try:
        logo_path = "DisasterLink-Systems-Capa.png"  # Caminho corrigido para a raiz do projeto
        logo_base64 = get_img_as_base64(logo_path)
        # HTML puro pra montar o header, com a imagem em base64 que a gente carregou.
        header_html = f"""
        <div class="header-container">
            <img src="data:image/png;base64,{logo_base64}" style="max-height: 150px; margin-right: 20px;">
            <div>
                <h1 class="main-title">DisasterLink IoT Dashboard</h1>
                <p class="subtitle">Monitoramento de Alertas Climáticos em Tempo Real</p>
            </div>
        </div>
        """
        st.markdown(header_html, unsafe_allow_html=True)
    except Exception as e:
        # Se der ruim ao carregar a imagem (ex: arquivo não encontrado), a gente mostra um título simples pra não quebrar a página.
        st.title("🌎 DisasterLink IoT Dashboard")
        st.caption("Monitoramento de Alertas Climáticos em Tempo Real")
        st.error(f"Erro ao carregar a logo: {e}")

# ──────────────────────────────────────────────────────────────────────────────
# VARIÁVEIS GLOBAIS (acessadas pelo callback MQTT)

# O Streamlit reroda o script todo a cada interação. Variáveis normais seriam resetadas.
# Mas a gente precisa que os dados recebidos pelo MQTT persistam.
# Por isso, usamos `deque` (uma lista com tamanho máximo) e variáveis globais que
# são atualizadas pela thread do MQTT e lidas pelo loop principal do Streamlit.

# Inicializamos os históricos com zeros pra evitar que o gráfico dê erro na primeira vez que a página carrega.
history_temperature = deque([0.0] * 5, maxlen=MAX_HISTORY)
history_humidity    = deque([0.0] * 5, maxlen=MAX_HISTORY)
history_windSpeed   = deque([0.0] * 5, maxlen=MAX_HISTORY)

# Dicionário pra guardar o último dado recebido do sensor. É o que aparece nos "cards" principais.
latest_data = {
    "city":        "—",
    "temperature": 0.0,
    "humidity":    0.0,
    "windSpeed":  0.0
}

# Variáveis pra controlar o status de anomalia.
# `latest_anomaly` é um booleano (True/False) que diz se há um alerta ativo.
# `latest_anomaly_type` e `msg` guardam os detalhes do alerta.
latest_anomaly      = False
latest_anomaly_type = ""
latest_anomaly_msg  = ""

# Lista para guardar os alertas que vêm da API .NET.
saved_alerts = []
last_alerts_update = datetime.now()

# Essa é a variável "sinalizadora". A thread do MQTT coloca ela como `True` quando chega um dado novo.
# O loop principal do Streamlit fica de olho nela. Se estiver `True`, ele atualiza a tela.
data_updated = False

# ──────────────────────────────────────────────────────────────────────────────
def on_connect(client, userdata, flags, rc, properties=None):
    """Callback para quando a conexão com o broker é estabelecida."""
    # Essa função é chamada automaticamente pela biblioteca Paho-MQTT quando a conexão é feita.
    if rc == 0:
        print("✅ Conectado ao broker MQTT com sucesso!")
        # A gente se inscreve no tópico aqui dentro. Assim, se a conexão cair e voltar, ele se inscreve de novo.
        client.subscribe(TOPIC)
        print(f"👂 Escutando o tópico: {TOPIC}")
    else:
        print(f"❌ Falha ao conectar ao broker, código de retorno: {rc}")

def on_subscribe(client, userdata, mid, granted_qos, properties=None):
    """Callback quando a inscrição em um tópico é confirmada."""
    print(f"✅ Inscrição confirmada para o tópico: {TOPIC}")

def on_message(client, userdata, msg):
    """
    Callback do MQTT, rodando em thread separada.
    Atualiza latest_data, history_*, dispara chamada à API de anomalia
    e seta data_updated.
    """
    # ESSA É A FUNÇÃO MAIS IMPORTANTE DA PARTE DE "TEMPO REAL".
    # Ela roda numa thread separada, fora do fluxo normal do Streamlit.
    # Toda vez que uma mensagem chega no tópico que a gente se inscreveu, essa função é executada.
    global latest_data, latest_anomaly, latest_anomaly_type, latest_anomaly_msg, data_updated

    print(f"📩 Mensagem recebida no tópico: {msg.topic}")
    print(f"📄 Payload bruto: {msg.payload}")

    try:
        # O dado chega como bytes, a gente decodifica pra string e depois converte de JSON pra um dicionário Python.
        payload = json.loads(msg.payload.decode())
        print(f"🔍 Payload decodificado: {payload}")
    except Exception as e:
        print("⚠️ Erro ao fazer json.loads no payload:", e)
        return

    # Extrai o nome da cidade do payload.
    city = payload.get("city", "N/A")
    latest_data["city"] = city

    # Debug pra gente ver no terminal o que chegou.
    print("⏰ MQTT chegou (thread):", payload)

    # Pega os dados dos sensores de dentro do payload.
    sensors = payload.get("sensors", {})
    t = sensors.get("temperature", None)
    h = sensors.get("humidity", None)
    ws = sensors.get("windSpeed", None)

    # Atualiza as variáveis globais com os novos dados.
    # O `deque` automaticamente joga fora o valor mais antigo se o histórico estiver cheio.
    if t is not None:
        latest_data["temperature"] = t
        history_temperature.append(t)
    if h is not None:
        latest_data["humidity"] = h
        history_humidity.append(h)
    if ws is not None:
        latest_data["windSpeed"] = ws
        history_windSpeed.append(ws)

    # AQUI ACONTECE A INTEGRAÇÃO COM A API DE ANOMALIA.
    # Pegamos os dados que acabaram de chegar e mandamos pra outra API (feita em FastAPI).
    # Essa API que tem a lógica pra decidir se os valores são uma anomalia ou não.
    try:
        # Monta o pacotinho de dados pra enviar pra API de anomalia.
        anomaly_data = {
            "city": city,
            "temperature": sensors.get("temperature", 0),
            "humidity": sensors.get("humidity", 0),
            "windSpeed": sensors.get("windSpeed", 0)
        }
        
        # O endpoint da nossa API de anomalia que está no ar, no Fly.io.
        endpoint_fly_io = "https://anomaly-api.fly.dev/anomaly"
        success = False
        
        try:
            # Faz a chamada POST pra API. Se ela não responder em 5 segundos, a gente desiste (timeout).
            print(f"🔌 Tentando conectar à API: {endpoint_fly_io}")
            resp = requests.post(endpoint_fly_io, json=anomaly_data, timeout=5) 
            print(f"✅ Conexão bem-sucedida! Código de status: {resp.status_code}")
            
            # A API de anomalia responde com um JSON. A gente pega a resposta aqui.
            result = resp.json()
            latest_anomaly = result.get("sent", False) # `sent` diz se o alerta foi enviado (se é uma anomalia)
            latest_anomaly_type = result.get("reason", "") # `reason` diz qual o tipo de anomalia (ex: "chuva forte")
            
            # Monta uma mensagem amigável pra mostrar na tela.
            if latest_anomaly:
                latest_anomaly_msg = f"Alerta '{latest_anomaly_type}' enviado com sucesso!"
            else:
                latest_anomaly_msg = result.get("reason", "Nenhuma anomalia ou erro no envio.")
            success = True
        # Bloco de tratamento de erros de conexão. Super importante pra não quebrar tudo se a API estiver fora do ar.
        except requests.exceptions.ConnectionError as e:
            print(f"❌ Erro de conexão ao tentar {endpoint_fly_io}: {e}")
            latest_anomaly_msg = f"Erro de conexão com API: {e}"
        except requests.exceptions.Timeout as e:
            print(f"❌ Timeout ao tentar {endpoint_fly_io}: {e}")
            latest_anomaly_msg = f"Timeout na API: {e}"
        except requests.exceptions.RequestException as e:
            print(f"❌ Erro inesperado na requisição para {endpoint_fly_io}: {e}")
            latest_anomaly_msg = f"Erro na API: {e}"
        except Exception as e:
            print(f"❌ Erro inesperado ao processar resposta de {endpoint_fly_io}: {e}")
            latest_anomaly_msg = f"Erro processando resposta da API: {e}"
        
        if not success:
            print(f"⚠️ Erro na chamada HTTP de anomalia para {endpoint_fly_io}")
            # Se deu erro, a gente garante que não tem nenhuma anomalia sendo mostrada.
            latest_anomaly = False
            latest_anomaly_type = ""
    except Exception as e:
        print("⚠️ Erro na chamada HTTP de anomalia:", e)
        # Se der qualquer outro erro, a gente também reseta o status da anomalia.
        latest_anomaly      = False
        latest_anomaly_type = ""
        latest_anomaly_msg  = ""

    # SINALIZAÇÃO!
    # Depois de fazer tudo, a gente avisa o loop principal que tem dados novos pra mostrar na tela.
    data_updated = True

def setup_mqtt_client():
    """
    Cria o client MQTT, configura callback, conecta no broker HiveMQ
    e subscreve no TOPIC. Retorna o client para persistir em session_state.
    """
    # Usando a API de callback v2 para evitar DeprecationWarning
    client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2)
    # Diz pro cliente quais funções devem ser chamadas pra cada evento (conexão, mensagem, etc).
    client.on_connect = on_connect
    client.on_subscribe = on_subscribe
    client.on_message = on_message
    client.connect(BROKER, PORT, 60)
    # client.loop_start() é o que cria a thread em background pra ficar ouvindo as mensagens do MQTT.
    # Isso é crucial pra não travar a interface do Streamlit.
    client.loop_start()
    return client

# ──────────────────────────────────────────────────────────────────────────────
# @st.cache_data é um "decorator" do Streamlit. Ele funciona como uma memória de curto prazo (cache).
# Se a função for chamada de novo com os mesmos parâmetros dentro do tempo `ttl` (time-to-live),
# o Streamlit não executa a função de novo, ele só retorna o resultado da última vez.
# Isso economiza tempo e chamadas de API, já que o dashboard atualiza a cada poucos segundos.
@st.cache_data(ttl=300) # Cache de 5 minutos (300s)
def get_openweather(city_name: str, api_key: str):
    """
    Busca dados de clima no OpenWeatherMap (cidade e chave de API).
    Retorna dict com keys: 'temp', 'humidity', 'wind_speed'.
    Se falhar, retorna {} e exibe aviso no Streamlit.
    """
    url = f"http://api.openweathermap.org/data/2.5/weather?q={city_name}&appid={api_key}&units=metric"
    try:
        r = requests.get(url, timeout=5)
        r.raise_for_status()  # Isso aqui lança um erro se a resposta for tipo 404 ou 500.
        data = r.json()
        return {
            "temp":       data["main"]["temp"],
            "humidity":   data["main"]["humidity"],
            "wind_speed": data["wind"]["speed"]
        }
    except ConnectionError:
        st.warning("🔌 Não foi possível conectar ao OpenWeatherMap.")
        return {}
    except Exception as e:
        st.warning(f"⚠️ Erro ao buscar dados no OpenWeatherMap: {e}")
        return {}

@st.cache_data(ttl=86400) # Cache de 24 horas (86400s), já que abrigos não mudam com frequência.
def get_abrigos_osm():
    """
    Busca abrigos via Overpass API (OpenStreetMap) usando
    primeiramente 'emergency=shelter'. Se não encontrar nada,
    tenta 'amenity=shelter'. Retorna DataFrame com colunas: 'nome', 'lat', 'lon'.
    """
    # A gente tenta duas buscas diferentes. A primeira é por abrigos de emergência.
    # Se não achar nada, tenta uma busca mais genérica por abrigos. É uma forma de ter um fallback.
    queries = [
        """
        [out:json][timeout:120];
        area["ISO3166-1"="BR"][admin_level=2];
        (
          node["emergency"="shelter"](area);
          way["emergency"="shelter"](area);
          relation["emergency"="shelter"](area);
        );
        out center;
        """,
        """
        [out:json][timeout:120];
        area["ISO3166-1"="BR"][admin_level=2];
        (
          node["amenity"="shelter"](area);
          way["amenity"="shelter"](area);
          relation["amenity"="shelter"](area);
        );
        out center;
        """
    ]

    url = "https://overpass-api.de/api/interpreter"
    for query in queries:
        try:
            resp = requests.post(url, data={"data": query})
            resp.raise_for_status()
            data = resp.json()
            elementos = data.get("elements", [])
            # Se a query retornou algum resultado, a gente processa e já sai do loop.
            if elementos:
                lista = []
                for el in elementos:
                    tags = el.get("tags", {})
                    nome = tags.get("name", "Sem nome")
                    # No OpenStreetMap, um local pode ser um ponto (node) ou uma área (way).
                    # A gente precisa tratar os dois casos pra pegar a coordenada (lat/lon).
                    if el["type"] == "node":
                        lat = el.get("lat")
                        lon = el.get("lon")
                    else:
                        centro = el.get("center", {})
                        lat = centro.get("lat")
                        lon = centro.get("lon")
                    if lat is not None and lon is not None:
                        lista.append({"nome": nome, "lat": lat, "lon": lon})
                # Retorna um DataFrame do Pandas, que é o formato que o `st.map` e `st.dataframe` gostam.
                return pd.DataFrame(lista)
        except Exception as e:
            print(f"⚠️ Erro ao buscar abrigos no OSM (tentativa): {e}")
            continue # Se der erro, a gente simplesmente tenta a próxima query da lista.

    return pd.DataFrame(columns=["nome", "lat", "lon"]) # Se nada der certo, retorna um DataFrame vazio.

# ──────────────────────────────────────────────────────────────────────────────
# COMPONENTES DA INTERFACE

# As funções `render_*` são responsáveis por desenhar partes específicas da tela.
# Elas pegam os dados e cospem HTML ou componentes do Streamlit.
# Manter isso em funções separadas deixa o código principal mais limpo.

def render_sensor_cards(data):
    """Renderiza os cards de sensores com valores atuais"""
    col1, col2, col3, col4 = st.columns(4) # Divide a tela em 4 colunas.
    
    with col1:
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">🏙️ Cidade</div>
            <div class="sensor-value">{data["city"]}</div>
        </div>
        """, unsafe_allow_html=True)
        
    with col2:
        temp = data["temperature"]
        temp_icon = "🥶" if temp < 10 else "🌡️" if temp < 30 else "🔥"
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">{temp_icon} Temperatura</div>
            <div class="sensor-value">{temp}°C</div>
        </div>
        """, unsafe_allow_html=True)
        
    with col3:
        hum = data["humidity"]
        hum_icon = "🌵" if hum < 30 else "💧"
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">{hum_icon} Umidade</div>
            <div class="sensor-value">{hum}%</div>
        </div>
        """, unsafe_allow_html=True)
        
    with col4:
        wind = data["windSpeed"]
        wind_icon = "🍃" if wind < 10 else "💨" if wind < 20 else "🌪️"
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">{wind_icon} Vel. Vento</div>
            <div class="sensor-value">{wind} m/s</div>
        </div>
        """, unsafe_allow_html=True)

def render_alert_status(is_alert, alert_type, alert_msg):
    """Renderiza o status de alerta atual"""
    # Se a variável `is_alert` for True, mostra o card de alerta vermelho e com animação.
    if is_alert:
        st.markdown(f"""
        <div class="alert-card">
            <div class="alert-title">🚨 ALERTA DETECTADO: {alert_type.upper()}</div>
            <div class="alert-message">{alert_msg}</div>
        </div>
        """, unsafe_allow_html=True)
    else:
        # Se não, mostra um card "inativo" verde, dizendo que tá tudo bem.
        st.markdown(f"""
        <div class="alert-card-inactive">
            <div class="alert-title">✅ Nenhuma anomalia detectada</div>
            <div class="alert-message">Todos os sensores reportando valores normais</div>
        </div>
        """, unsafe_allow_html=True)

def render_atuador_control():
    """Renderiza o controle do atuador fixo em modo automático"""
    # Esta parte da interface mostra o controle do atuador.
    # No momento, ele está "chumbado" no modo automático. Não tem botão pra ligar/desligar manual.
    st.markdown('<div class="divider"></div>', unsafe_allow_html=True)
    st.markdown('<h3 style="color: #50B5FF;">🔧 Controle do Atuador</h3>', unsafe_allow_html=True)
    
    col1, col2 = st.columns([3, 1])
    
    with col1:
        st.markdown("""
        <div style="background-color: #161B22; padding: 15px; border-radius: 10px; margin-top: 10px;">
            <span style="font-size: 1.0rem; color: #F0F6FC;">Modo de Operação: <b>Automático</b></span>
            <p style="font-size: 0.85rem; color: #A3B9CC; margin-top: 8px;">
                O sistema ativa o atuador automaticamente quando uma anomalia é detectada.
            </p>
        </div>
        """, unsafe_allow_html=True)
    
    with col2:
        # Status sempre ON e verde
        status = "ON"
        color = "#50C878"  # Sempre verde
        st.markdown(f"""
        <div style="background-color: #161B22; padding: 15px; border-radius: 10px; text-align: center; margin-top: 10px;">
            <span style="font-size: 0.8rem; color: #A3B9CC;">STATUS</span>
            <h3 style="color: {color}; margin: 5px 0;">{status}</h3>
        </div>
        """, unsafe_allow_html=True)
    
    # O atuador no dashboard é só visual, a lógica de verdade acontece no loop principal
    # que publica a mensagem MQTT pra ligar/desligar.
    return "Automático"

def render_openweather_data(weather_data):
    """Renderiza os dados do OpenWeatherMap"""
    # Se a função que busca os dados do tempo não retornou nada, a gente nem tenta renderizar.
    if not weather_data:
        return
        
    st.markdown('<div class="divider"></div>', unsafe_allow_html=True)
    st.markdown('<h3 style="color: #50B5FF;">🌤️ Dados Meteorológicos Oficiais</h3>', unsafe_allow_html=True)
    
    col1, col2, col3 = st.columns(3)
    
    with col1:
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">🌡️ Temperatura (São Paulo)</div>
            <div class="sensor-value">{weather_data["temp"]}°C</div>
        </div>
        """, unsafe_allow_html=True)
        
    with col2:
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">💧 Umidade (São Paulo)</div>
            <div class="sensor-value">{weather_data["humidity"]}%</div>
        </div>
        """, unsafe_allow_html=True)
        
    with col3:
        st.markdown(f"""
        <div class="sensor-card">
            <div class="sensor-label">💨 Vel. Vento (São Paulo)</div>
            <div class="sensor-value">{weather_data["wind_speed"]} m/s</div>
        </div>
        """, unsafe_allow_html=True)

def render_abrigos(df_abrigos):
    """Renderiza o mapa e tabela de abrigos"""
    if df_abrigos.empty:
        return
        
    st.markdown('<div class="divider"></div>', unsafe_allow_html=True)
    st.markdown('<h3 style="color: #50B5FF;">🏠 Abrigos e Pontos de Apoio</h3>', unsafe_allow_html=True)
    
    col1, col2 = st.columns([2, 1])
    
    with col1:
        st.markdown('<div class="map-container">', unsafe_allow_html=True)
        st.map(df_abrigos[["lat", "lon"]])
        st.markdown('</div>', unsafe_allow_html=True)
        
    with col2:
        st.dataframe(
            df_abrigos[["nome"]]
            .rename(columns={"nome": "Nome do Abrigo"})
            .head(10),
            hide_index=True,
            use_container_width=True
        )

def render_status_bar():
    """Renderiza a barra de status na parte inferior da dashboard"""
    now = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
    st.markdown(f"""
    <div class="status-bar">
        <span>DisasterLink Systems • Monitoramento IoT</span>
        <span>Última atualização: {now}</span>
    </div>
    """, unsafe_allow_html=True)

@st.cache_data(ttl=30)  # Cache curto de 30s. A gente quer ver os alertas novos com certa frequência.
def get_saved_alerts():
    """
    Busca os alertas salvos na API C#.
    Retorna uma lista de alertas ou uma lista vazia em caso de erro.
    """
    # Essa função bate na nossa API .NET pra pegar o histórico completo de alertas.
    try:
        resp = requests.get(C_SHARP_API_URL, timeout=5)
        resp.raise_for_status()
        return resp.json()
    except Exception as e:
        # Se a API .NET estiver fora, a gente só avisa no console e retorna uma lista vazia.
        # Assim o resto do dashboard continua funcionando.
        print(f"⚠️ Erro ao buscar alertas na API C#: {e}")
        return []

def render_saved_alerts(alerts):
    """
    Renderiza a lista de alertas salvos na API C#
    """
    # Se a lista de alertas estiver vazia, mostra uma mensagem amigável.
    if not alerts:
        st.info("Nenhum alerta climático registrado no banco de dados.")
        return
    
    st.markdown('<div class="divider"></div>', unsafe_allow_html=True)
    st.markdown('<h3 style="color: #50B5FF;">🚨 Histórico de Alertas</h3>', unsafe_allow_html=True)
    
    st.markdown('<div class="chart-container">', unsafe_allow_html=True)
    
    # Usamos a biblioteca Pandas pra manipular os dados dos alertas. Facilita muito a vida.
    alerts_df = pd.DataFrame(alerts)
    
    # Renomeia as colunas do JSON da API para nomes mais bonitinhos de se mostrar na tela.
    if not alerts_df.empty:
        rename_map = {
            "id": "ID",
            "cidade": "Cidade", 
            "tipoAlerta": "Tipo de Alerta",
            "temperatura": "Temperatura (°C)",
            "umidade": "Umidade (%)",
            "vento": "Vento (m/s)",
            "dataHora": "Data/Hora"
        }
        
        display_columns = ["cidade", "tipoAlerta", "temperatura", "umidade", "vento", "dataHora"]
        df_display = alerts_df[display_columns].rename(columns=rename_map)
        
        # Ordena pra mostrar os alertas mais novos primeiro.
        df_display = df_display.sort_values(by="Data/Hora", ascending=False)
        
        # Faz uma contagem de quantos alertas cada cidade já teve.
        st.markdown('<div class="chart-title">📊 Distribuição de Alertas por Cidade</div>', unsafe_allow_html=True)
        city_counts = alerts_df["cidade"].value_counts().reset_index()
        city_counts.columns = ["Cidade", "Quantidade de Alertas"]
        
        # Gera um gráfico de barras com a contagem.
        st.bar_chart(city_counts.set_index("Cidade"))
        
        # Título da tabela
        st.markdown('<div class="chart-title">📋 Últimos Alertas Registrados</div>', unsafe_allow_html=True)
        
        # Mostra a tabela com os 10 últimos alertas.
        st.dataframe(
            df_display.head(10),
            hide_index=True,
            use_container_width=True
        )
        
        st.caption(f"Total de {len(alerts_df)} alertas registrados. Atualizado em: {datetime.now().strftime('%d/%m/%Y %H:%M:%S')}")
    
    st.markdown('</div>', unsafe_allow_html=True)

# ──────────────────────────────────────────────────────────────────────────────
# INICIALIZAÇÃO DO STREAMLIT

st.set_page_config(
    page_title="DisasterLink IoT Dashboard",
    layout="wide", # "wide" faz o conteúdo ocupar a tela toda.
    initial_sidebar_state="collapsed" # Esconde a barra lateral por padrão.
)

# Aplica nosso CSS customizado que definimos lá em cima.
local_css()

# PARTE IMPORTANTE SOBRE ESTADO NO STREAMLIT:
# O script do Streamlit roda do começo ao fim toda vez que o usuário interage com a página.
# Se a gente simplesmente fizesse `mqtt_client = setup_mqtt_client()`, ele ia tentar criar uma nova conexão
# a cada 2 segundos, o que seria um desastre.
# `st.session_state` é um "lugar" especial que o Streamlit oferece pra guardar variáveis
# que sobrevivem entre as rerodagens do script.
# Então, a gente verifica se o cliente MQTT já não existe no `session_state` antes de criar um novo.
if 'mqtt_client' not in st.session_state:
    st.session_state.mqtt_client = setup_mqtt_client()

# ──────────────────────────────────────────────────────────────────────────────
# RENDERIZAÇÃO DA INTERFACE

# Cabeçalho
create_header()

# st.empty() cria um "placeholder" na tela. É um container vazio que a gente
# pode preencher ou atualizar depois sem ter que redesenhar a página inteira.
# A gente vai usar isso pra atualizar os dados dos sensores e o status do alerta.
json_container = st.empty()

# Container principal para dados dos sensores e alertas
main_container = st.container()
with main_container:
    st.markdown("<h3 style='color: #50B5FF;'>🔍 Leituras dos Sensores em Tempo Real</h3>", unsafe_allow_html=True)
    
    # Cards de sensores
    sensor_cards = st.empty()
    
    # Status de alerta
    alert_status = st.empty()

# Container para controle do atuador
atuador_container = st.container()
with atuador_container:
    render_atuador_control()

# Container para alertas salvos na API
st.markdown("<div style='height: 20px;'></div>", unsafe_allow_html=True)  # Espaçador
alerts_container = st.container()
with alerts_container:
    # Busca os alertas da API. A função `get_saved_alerts` tem cache,
    # então essa chamada só vai realmente na API a cada 30 segundos.
    saved_alerts = get_saved_alerts()
    render_saved_alerts(saved_alerts)

# Container para abrigos
abrigos_container = st.container()
with abrigos_container:
    # Busca abrigos no OpenStreetMap. Também usa cache.
    df_abrigos = get_abrigos_osm()
    df_abrigos_com_nome = df_abrigos[df_abrigos["nome"] != "Sem nome"]
    render_abrigos(df_abrigos_com_nome)

# ──────────────────────────────────────────────────────────────────────────────
# LOOP PRINCIPAL: atualiza interface quando data_updated == True

# Guardamos os placeholders em variáveis pra ficar mais fácil de usar dentro do loop.
placeholder_status = json_container
placeholder_cards = sensor_cards
placeholder_alert = alert_status

@st.cache_data(ttl=10)  # Cache bem curto. A gente quer verificar isso com frequência.
def get_latest_alert():
    """
    Busca o alerta mais recente na API C# para verificar se há alguma anomalia atual.
    Retorna um dict com informações sobre o alerta mais recente.
    """
    # Essa função é uma checagem de segurança. Se o dashboard for aberto e já existir
    # um alerta recente no banco de dados, a gente já mostra o status de alerta,
    # mesmo sem ter recebido uma nova mensagem do MQTT ainda.
    try:
        resp = requests.get(C_SHARP_API_URL, timeout=5)
        resp.raise_for_status()
        alerts = resp.json()
        
        if not alerts:
            return None
            
        # Usa Pandas de novo pra achar o alerta mais recente.
        df = pd.DataFrame(alerts)
        if "dataHora" in df.columns:
            # Converte a string de data/hora para um objeto datetime pra poder ordenar corretamente.
            df["datetime"] = pd.to_datetime(df["dataHora"], format="%d/%m/%Y %H:%M")
            df = df.sort_values(by="datetime", ascending=False)
            
            # Pega o primeiro da lista (o mais recente).
            latest = df.iloc[0].to_dict()
            
            # Checa se o alerta é realmente recente (nas últimas 2 horas).
            # Não adianta mostrar um alerta de 3 dias atrás como se estivesse ativo agora.
            alert_time = latest["datetime"]
            now = pd.Timestamp.now()
            if now - alert_time < pd.Timedelta(hours=2):
                return latest
                
        return None
    except Exception as e:
        print(f"⚠️ Erro ao buscar último alerta na API C#: {e}")
        return None

try:
    # Renderização inicial. Quando a página carrega pela primeira vez, a gente
    # já mostra os cards e o status de alerta com os valores padrão.
    with placeholder_cards:
        render_sensor_cards(latest_data)
        
    with placeholder_alert:
        render_alert_status(latest_anomaly, latest_anomaly_type, latest_anomaly_msg)
    
    # O CORAÇÃO DO DASHBOARD.
    # Este loop `while True` é o que faz a mágica da atualização em tempo real acontecer.
    while True:
        # 1. CHECAGEM POR DADOS NOVOS DO SENSOR (via MQTT)
        # A gente só olha a flag `data_updated`. A thread do MQTT que faz o trabalho sujo.
        if data_updated:
            # Se a flag for True, significa que a função `on_message` rodou.
            # Então, a gente atualiza os componentes na tela que precisam mudar.
            
            # Usa o placeholder `placeholder_cards` pra redesenhar só os cards de sensores.
            with placeholder_cards:
                render_sensor_cards(latest_data)
                
            # Usa o placeholder `placeholder_alert` pra redesenhar só o status do alerta.
            with placeholder_alert:
                render_alert_status(latest_anomaly, latest_anomaly_type, latest_anomaly_msg)
            
            # Este container é pra gente poder debugar e ver o JSON dos dados recebidos.
            with placeholder_status:
                if latest_anomaly:
                    st.json({
                        "city":        latest_data["city"],
                        "temperature": latest_data["temperature"],
                        "humidity":    latest_data["humidity"],
                        "windSpeed":   latest_data["windSpeed"],
                        "anomaly": {
                            "type":    latest_anomaly_type,
                            "message": latest_anomaly_msg
                        }
                    })
                else:
                    st.json({
                        "city":        latest_data["city"],
                        "temperature": latest_data["temperature"],
                        "humidity":    latest_data["humidity"],
                        "windSpeed":   latest_data["windSpeed"],
                        "anomaly":     False
                    })

            # LÓGICA DO ATUADOR:
            # Baseado no status da anomalia (`latest_anomaly`), a gente decide se liga ou desliga o atuador.
            cmd = "ON" if latest_anomaly else "OFF"
            # Publica uma mensagem MQTT no tópico do atuador. O ESP32 que estiver escutando
            # esse tópico vai receber o comando "ON" ou "OFF" e acionar o relé/motor/etc.
            st.session_state.mqtt_client.publish(
                "disasterlink/sim/esp32/atuador",
                json.dumps({"command": cmd})
            )

            # Reseta a flag para `False`. Agora a gente espera a próxima mensagem do MQTT chegar.
            data_updated = False
        
        # 2. CHECAGEM POR ALERTAS EXISTENTES (via API .NET)
        # Isso garante que o status de alerta seja atualizado mesmo se não houver novas mensagens MQTT.
        # É a nossa checagem de segurança que roda a cada `time.sleep` segundos.
        latest_alert = get_latest_alert()
        if latest_alert is not None:
            # Se a função encontrou um alerta recente no banco de dados...
            if not latest_anomaly:  # ...e a gente ainda não está mostrando um alerta...
                # ...então a gente ativa o modo de alerta no dashboard.
                latest_anomaly = True
                latest_anomaly_type = latest_alert.get("tipoAlerta", "Desconhecido")
                # Aqui a mensagem de alerta vem dos dados da API .NET.
                latest_anomaly_msg = f"Alerta detectado para {latest_alert.get('cidade')}: {latest_alert.get('descricao')}"
                
                # Força a atualização da interface com as novas informações de alerta.
                with placeholder_alert:
                    render_alert_status(latest_anomaly, latest_anomaly_type, latest_anomaly_msg)
                
                # Também manda ligar o atuador, por segurança.
                st.session_state.mqtt_client.publish(
                    "disasterlink/sim/esp32/atuador",
                    json.dumps({"command": "ON"})
                )
                
                print(f"✅ Status de alerta atualizado com base em alerta salvo: {latest_anomaly_type}")

        # Pausa o loop por 3 segundos. Isso evita que o loop rode sem parar e consuma 100% da CPU.
        # É basicamente a "taxa de atualização" do nosso dashboard.
        time.sleep(3)

except KeyboardInterrupt:
    # Se a gente apertar Ctrl+C no terminal, o programa para de forma limpa.
    pass
