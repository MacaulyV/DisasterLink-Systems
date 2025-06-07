import os, time, json, itertools, requests, unicodedata
import paho.mqtt.client as mqtt

# Configurações principais do projeto
BROKER = "broker.hivemq.com"       # Broker MQTT público (não precisa autenticação)
PORT   = 1883                      # Porta padrão do MQTT
BASE_TOPIC = "disasterlink/real"   # Prefixo dos tópicos MQTT (pra não misturar com outros projetos)

API_KEY = os.getenv("OWM_API_KEY", "3593e408dfaf865658df8fd5ccdf795d")
UNITS   = "metric"                 # Pra receber °C, mm e % (fica mais fácil de ler)
SLEEP_BETWEEN_CITIES = 3           # Pausa entre cada cidade (pra não estourar limite da API free)

# Lista das cidades pra puxar clima
# Coloquei capitais e cidades de risco, mas pode editar aqui de boas
CITIES = [
    ("São Paulo", -23.5505, -46.6333),
    ("Rio de Janeiro", -22.9068, -43.1729),
    ("Belo Horizonte", -19.9167, -43.9345),
    ("Porto Alegre", -30.0346, -51.2177),
    ("Salvador", -12.9714, -38.5014),
    ("Brasília", -15.7801, -47.9292),
    ("Fortaleza", -3.7319, -38.5267),
    ("Curitiba", -25.4284, -49.2733),
    ("Manaus", -3.1190, -60.0217),
    ("Recife", -8.0476, -34.8770),
    ("Goiânia", -16.6869, -49.2648),
    ("Belém", -1.4557, -48.4902),
    ("Guarulhos", -23.4543, -46.5337),
    ("Campinas", -22.9071, -47.0632),
    ("Natal", -5.7945, -35.2090),
    ("São Luís", -2.5327, -44.3068),
    ("Maceió", -9.6498, -35.7089),
    ("João Pessoa", -7.1150, -34.8631),
    ("Teresina", -5.0892, -42.8096),
    ("Campo Grande", -20.4435, -54.6478),
    ("São José dos Campos", -23.2237, -45.9009),
    ("Ribeirão Preto", -21.1767, -47.8208),
    ("Uberlândia", -18.9126, -48.2755),
    ("Sorocaba", -23.5017, -47.4581),
    ("Contagem", -19.9316, -44.0536),
    ("Aracaju", -10.9472, -37.0731),
    ("Feira de Santana", -12.2578, -38.9556),
    ("Cuiabá", -15.6014, -56.0979),
    ("Joinville", -26.3044, -48.8467),
    ("Juiz de Fora", -21.7645, -43.3493),
    ("Londrina", -23.3105, -51.1594),
    ("Aparecida de Goiânia", -16.8198, -49.2469),
    ("Porto Velho", -8.7612, -63.9004),
    ("Ananindeua", -1.3656, -48.3722),
    ("Serra", -20.1289, -40.3078),
    ("Niterói", -22.8832, -43.1034),
    ("Macapá", 0.0349, -51.0504),
    ("São José do Rio Preto", -20.8117, -49.3762),
    ("Mauá", -23.6677, -46.4613),
    ("São João de Meriti", -22.8038, -43.3722),
    ("Santos", -23.9608, -46.3339),
    ("Mogi das Cruzes", -23.5225, -46.1856),
    ("Betim", -19.9678, -44.1977),
    ("Diadema", -23.6861, -46.6227),
    ("Campina Grande", -7.2219, -35.8731),
    ("Jundiaí", -23.1857, -46.8978),
    ("Maringá", -23.4273, -51.9375),
    ("Montes Claros", -16.7282, -43.8651),
    ("Anápolis", -16.3287, -48.9534),
    ("Carapicuíba", -23.5235, -46.8407),
    ("Rio Branco", -9.9754, -67.8249),
    ("Piracicaba", -22.7253, -47.6493),
    ("Bauru", -22.3145, -49.0607),
    ("Vitória", -20.2976, -40.2958),
    ("Caucaia", -3.7319, -38.6557),
    ("Itaquaquecetuba", -23.4833, -46.3494),
    ("São Vicente", -23.9608, -46.3919),
    ("Caruaru", -8.2845, -35.9701),
    ("Blumenau", -26.9184, -49.0687),
    ("Franca", -20.5389, -47.4019),
    ("Petrópolis", -22.5044, -43.1789),
    ("Ponta Grossa", -25.0911, -50.1668),
    ("Cariacica", -20.2639, -40.3084),
    ("Vila Velha", -20.3297, -40.2925),
    ("Canoas", -29.9178, -51.1836),
    ("Pelotas", -31.7719, -52.3426),
    ("Vitória da Conquista", -14.8614, -40.8441),
    ("Ribeirão das Neves", -19.7669, -44.0869),
    ("Uberaba", -19.7477, -47.9392),
    ("Paulista", -7.9408, -34.8731),
    ("Cascavel", -24.9559, -53.4552),
    ("Praia Grande", -24.0084, -46.4120),
    ("São José dos Pinhais", -25.5349, -49.2056),
    ("Guarujá", -23.9937, -46.2564),
    ("Taubaté", -23.0264, -45.5553),
    ("Limeira", -22.5647, -47.4017),
    ("Santarém", -2.4431, -54.7083),
    ("Camaçari", -12.6992, -38.3236),
    ("Suzano", -23.5448, -46.3112),
    ("Taboão da Serra", -23.6019, -46.7526),
    ("São Leopoldo", -29.7545, -51.1497),
    ("Mossoró", -5.1883, -37.3441),
    ("Várzea Grande", -15.6467, -56.1325),
    ("Santa Maria", -29.6842, -53.8069),
    ("Gravataí", -29.9443, -50.9939),
    ("Foz do Iguaçu", -25.5163, -54.5854),
    ("Viamão", -30.0819, -51.0234),
    ("Sumaré", -22.8214, -47.2668),
    ("Palmas", -10.1843, -48.3336),
    ("Parnamirim", -5.9116, -35.2707),
    ("São Carlos", -22.0175, -47.8910),
    ("Marabá", -5.3689, -49.1258),
    ("Imperatriz", -5.5185, -47.4777),
    ("Castanhal", -1.2976, -47.9167),
    ("Divinópolis", -20.1445, -44.8939),
    ("Itabuna", -14.7874, -39.2781),
    ("Juazeiro do Norte", -7.2306, -39.3137),
    ("Marília", -22.2178, -49.9505),
    ("São Caetano do Sul", -23.6229, -46.5548),
    ("Itapevi", -23.5489, -46.9347),
    ("Luziânia", -16.2520, -47.9502),
    ("Hortolândia", -22.8528, -47.2143),
    ("Cabo Frio", -22.8894, -42.0286),
    ("Criciúma", -28.6775, -49.3697),
    ("Rio Verde", -17.7979, -50.9187),
    ("Cachoeiro de Itapemirim", -20.8462, -41.1198),
    ("Boa Vista", 2.8195, -60.6714),
    ("Palhoça", -27.6453, -48.6697),
    ("São Gonçalo", -22.8269, -43.0539),
    ("São Bernardo do Campo", -23.6939, -46.5650),
    ("Duque de Caxias", -22.7859, -43.3118),
    ("Nova Iguaçu", -22.7592, -43.4511),
    ("Santo André", -23.6543, -46.5333),
]

# ───────────────────────────────────────────────────────────────
# Função pra deixar o nome da cidade em um formato seguro pro tópico MQTT
def slug(text: str) -> str:
    """Transforma nome da cidade em slug (ex: 'São Paulo' → 'sao_paulo')."""
    text = unicodedata.normalize("NFKD", text).encode("ascii", "ignore").decode()
    return text.lower().replace(" ", "_")

# Função pra pegar os dados de clima da API do OpenWeatherMap
def get_weather(lat: float, lon: float) -> dict:
    """Chama a API de clima com latitude e longitude."""
    url = (
        "http://api.openweathermap.org/data/2.5/weather"
        f"?lat={lat}&lon={lon}&appid={API_KEY}&units={UNITS}"
    )
    r = requests.get(url, timeout=5)   # timeout pra não travar se cair a API
    r.raise_for_status()               # lança erro se a resposta não for 200
    return r.json()

# Função principal do script
def main() -> None:
    client = mqtt.Client()                 # Cria o client MQTT
    client.connect(BROKER, PORT, 60)       # Conecta no broker

    print("🌐 Conectado ao broker, iniciando publicação…")

    # Vai rodando as cidades em loop infinito, tipo um carrossel
    for city, lat, lon in itertools.cycle(CITIES):
        try:
            data = get_weather(lat, lon)                   # Busca o clima atual
            temp = data["main"]["temp"]                    # Temperatura
            hum  = data["main"]["humidity"]                # Umidade
            wind_speed = data["wind"]["speed"]             # Velocidade do vento

            # Se quiser usar chuva como "nível de água", pode descomentar esse bloco abaixo:
            # rain_mm = (
            #     data.get("rain", {}).get("1h")
            #     or data.get("rain", {}).get("3h", 0) / 3
            #     or 0.0
            # )
            # water_lvl = round(precip_to_water_level(rain_mm), 1)

            payload = {
                "city": city,
                "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ"),    # Timestamp no formato ISO
                "sensors": {
                    "temperature": round(float(temp), 1),
                    "humidity":    round(float(hum), 1),
                    "windSpeed":   round(float(wind_speed), 1)
                }
            }

            topic = f"{BASE_TOPIC}/{slug(city)}/sensor"    # Monta o tópico MQTT específico da cidade
            client.publish(topic, json.dumps(payload))     # Publica os dados no MQTT
            print("✅", topic, payload)                    # Loga o que foi enviado

        except Exception as e:
            print("⚠️  Falha ao obter/publicar dados para", city, ":", e)    # Se der ruim, mostra o erro

        time.sleep(SLEEP_BETWEEN_CITIES)   # Espera um tempo antes de passar pra próxima cidade

# Roda o main se o script for chamado direto (não importado)
if __name__ == "__main__":
    if API_KEY == "SUA_CHAVE_AQUI":
        raise RuntimeError("Defina a variável de ambiente OWM_API_KEY.")  # Evita rodar sem API Key
    main()