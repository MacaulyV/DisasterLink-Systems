![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/155f81d4-12a3-4e55-9f6e-6b7c8041b3f8)

# 🛰️ DisasterLink IoT — Monitoramento Climático Inteligente

## 📚 **Sobre o Projeto**

O projeto **DisasterLink-IoT** é uma solução que simula um sistema de monitoramento climático inteligente, usando conceitos de IoT e integração em tempo real para identificar riscos ambientais em cidades brasileiras.

Ele envolve:

- **Simulação de sensores climáticos** para várias cidades (via OpenWeatherMap)
- **Envio de dados** para uma plataforma via MQTT
- **Processamento inteligente** para gerar alertas climáticos via FastAPI
- **Dashboard moderno** (Streamlit) para monitoramento e visualização dos dados e alertas, tudo em tempo real

**Tudo isso conectado usando Python e várias bibliotecas robustas — 100% replicável com sensores simulados, sem necessidade de hardware físico.**

---

<details>
<summary><h2>**🏗️ Descrição Geral da Solução DisasterLink Systems**</h2></summary>

O **DisasterLink Systems** é uma solução digital **colaborativa e inteligente** focada em situações de emergência pós-desastres naturais — enchentes, deslizamentos, temporais, incêndios, entre outros cenários críticos que afetam cidades brasileiras.

A solução visa **integrar cidadãos, autoridades e tecnologia**, centralizando:

- Informação sobre abrigos e pontos de apoio.
- Mapeamento de doações e necessidades.
- Comunicação eficiente e automatizada via alertas.
- Recomendação personalizada baseada em IA para direcionar quem precisa de ajuda ao recurso certo.

---

## 👥 **Público-Alvo**

- Moradores de áreas de risco
- Defesa Civil e órgãos municipais/estaduais
- Voluntários e ONGs
- Toda a comunidade afetada

---

## 🤝 **Lógica de Integração da Solução — Como Tudo Colabora**

### 🧩 **Arquitetura Modular, Escalável e Sinérgica**

A **DisasterLink** é construída sobre **quatro módulos principais**, cada um com responsabilidade própria, mas todos orquestrados para entregar monitoramento, resposta e inteligência em tempo real para situações de emergência.

---

### 1️⃣ **API Central (.NET C#)**

**🧠 Papel:**

Cérebro da plataforma. Controla as regras de negócio, segurança (autenticação JWT), auditoria de operações, persistência de dados, integração com IA e orquestração de alertas.

**🔌 Integração:**

- Serve de ponte única entre todos os módulos.
- Recebe e processa cadastros, logins, buscas, recomendações, registros de doação, emissão de alertas, etc.
- Exige token JWT para ações sensíveis.
- Responsável por auditar tudo que acontece na plataforma.

---

### 2️⃣ **Aplicativo Mobile (React Native)**

**👤 Papel:**

Principal canal para o cidadão.

Permite visualizar abrigos e pontos de coleta, receber alertas, buscar recomendações, cadastrar-se e participar de doações/voluntariado.

**🔗 Integração:**

- Consome exclusivamente os endpoints da **API central**.
- Nunca armazena dados críticos localmente: tudo sincronizado em tempo real via API.
- Segurança e consistência garantidas.

---

### 3️⃣ **Painel Web/Admin (Java Spring Boot + Thymeleaf)**

**🛡️ Papel:**

Plataforma de administração para gestores, Defesa Civil e órgãos oficiais.

Garante governança, controle e auditoria dos recursos do sistema.

**🔗 Integração:**

- Também consome a **API central** (consultas e gestão).
- Possui endpoints próprios para autenticação e cadastro de administradores.
- Só admins autenticados podem acessar recursos críticos (ex: criar/excluir abrigos, emitir alertas).
- Toda autorização é centralizada via JWT.

---

### 4️⃣ **Módulo IoT & Inteligência Climática (Python — MQTT & FastAPI)**

**🌪️ Papel:**

Responsável pelo **monitoramento automatizado do clima** em dezenas de cidades, detecção de anomalias ambientais em tempo real e disparo automático de alertas críticos.

**🚦 Integração:**

- Sensores simulados (ou reais) coletam dados do clima (OpenWeatherMap) e publicam via MQTT.
- A **API de Anomalia (FastAPI)** consome os dados direto do broker MQTT, analisa em tempo real e dispara alertas para a **API central** quando identifica situações de risco.
- Dashboard web (Streamlit) conecta ao mesmo broker e às APIs, exibindo todo o ecossistema ao vivo para acompanhamento.

---

### 🧬 **Como Tudo Funciona Junto**

- **Usuário comum (mobile)** acessa a API para visualizar abrigos/pontos/alertas, fazer login/cadastro, receber recomendações.
- **Admin (painel web)** acessa a API para gerenciar recursos, cadastrar entidades, emitir alertas e auditar operações.
- **Módulo IoT** opera 24/7, monitorando condições ambientais e disparando alertas automáticos sem intervenção humana, alimentando a API central.
- **Dashboard** permite visualização em tempo real de tudo que está acontecendo — desde leituras de sensores até alertas salvos no sistema.

---

### 🌟 **Benefícios da Integração**

- **🔒 Segurança Total:**
    
    Todas as operações sensíveis são protegidas por autenticação/autorização centralizada.
    
- **⏱️ Tempo Real e Consistência:**
    
    Mobile, web e IOT consomem e publicam dados sempre atualizados, tudo auditado e centralizado.
    
- **📈 Inteligência e Automação:**
    
    Regras de negócio, recomendações (IA/ML.NET) e alertas são orquestrados para máxima eficiência, sem risco de duplicidade ou atraso.
    
- **⚙️ Facilidade de Manutenção e Evolução:**
    
    Os módulos evoluem de forma independente, desde que mantenham o contrato com a API central.
    
</details>

## 🌎 Como Funciona a Solução DisasterLink IoT

![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/c4d07999-f928-48b1-a7f7-560ba49a1b82)

---

### 🧩 **Arquitetura Integrada & Fluxo de Dados**

O projeto roda em três pilares principais:

### **1️⃣ Simulador de Sensores (`src/weather_to_mqtt.py`)**

- **Simula sensores** distribuídos em ~100 cidades brasileiras, coletando dados reais de clima via OpenWeatherMap.
- **Publica esses dados em tópicos MQTT exclusivos** por cidade (ex: `disasterlink/real/sao_paulo/sensor`).
- **Publicação direta no broker MQTT**, sem depender da dashboard.

### **2️⃣ Analisador de Anomalias (`src/anomaly.py`)**

- **Recebe dados dos sensores simulados via MQTT**, direto do broker.
- **Analisa em tempo real**: identifica situações críticas (calor extremo, vento perigoso, umidade baixa, etc).
- **Gera e salva alertas automaticamente** (registrando na API central em .NET) sem precisar da dashboard ativa.
- **Processamento é contínuo**: a API se mantém escutando e salvando eventos assim que recebe, mesmo que ninguém esteja visualizando nada.

### **3️⃣ Dashboard Interativo (`src/dashboard.py`)**

- **Visualiza todo o ecossistema funcionando**: conecta ao broker MQTT, escuta todas as mensagens dos sensores e mostra as condições ao vivo.
- **Exibe métricas, gráficos e alertas** em tempo real.
- **Não interfere no fluxo dos dados**, serve apenas pra visualização e acompanhamento, podendo ser desligada sem interromper o sistema.

---

### 🚀 **Fluxo Completo do Sistema**

1. **Sensores Virtuais** publicam dados de clima em tópicos MQTT.
2. **API de Anomalia** consome os dados MQTT, analisa, e salva alertas automaticamente na API .NET.
3. **Dashboard** apenas consome o MQTT e as APIs para exibir tudo de forma visual, mas não é necessária pro funcionamento dos alertas.

---

### 🧠 **O que é Simulado? O que é Real?**

- **Sensores:** Simulados (dados reais via API, mas publicação virtual).
- **Tráfego MQTT:** 100% real, igual a um sistema de IoT.
- **API de Anomalia:** Real, opera em nuvem, consome MQTT em tempo real sem depender da dashboard.
- **Dashboard:** Visualização em tempo real; não é responsável por análise nem geração de alertas.

## 🏗️ **Estrutura do Projeto**

```

src/
├── weather_to_mqtt.py     # Simulador de sensores e publisher MQTT (produção/nuvem)
├── anomaly.py             # API de análise/anomalia (produção/nuvem)
├── dashboard.py           # Dashboard Streamlit (roda LOCALMENTE)          
requirements.txt
.gitignore
fly.toml
README.md
```

---

## ⚙️ Bibliotecas e Tecnologias

- **Python 3.11:** Linguagem central do projeto.
- **MQTT (`paho-mqtt`)**: Protocolo leve pra comunicação máquina-a-máquina, ideal pra IoT.
- **FastAPI:** Framework pra criar APIs REST modernas e rápidas.
- **Streamlit:** Framework pra criar dashboards web interativos em Python, focado em ciência de dados e monitoramento.
- **OpenWeatherMap:** Fonte dos dados climáticos (sensores simulados).
- **HTTPX, Requests:** Consumo de APIs HTTP.
- **Pandas/Numpy:** Manipulação e análise de dados para exibir no dashboard

---

## 🚦 **O que você precisa rodar localmente para testar**

> 🖥️ Só precisa rodar a Dashboard!
> 
- Os scripts de simulação de sensores e a API de Anomalia já estão **em produção**.
- Ao iniciar o dashboard, ele **automaticamente se conecta ao MQTT e à API de Anomalia na nuvem**.
- **Não precisa Executar nada extra além do dashboard**.

---

## 🧪 **Como rodar o Dashboard local**

### **Pré-requisitos**

- Python **3.11.x** instalado

### **Passo a passo**

1. **Clone o projeto:**
    
    ```bash
    
    git clone https://github.com/SEU-USUARIO/disasterlink-iot.git
    cd disasterlink-iot
    ```
    
2. **Crie o ambiente virtual:**
    
    ```bash
    
    C:\Users\SeuUsuario\AppData\Local\Programs\Python\Python311\python.exe -m venv .venv
    ```
    
3. **Ative o ambiente virtual:**
    
    ```bash
    
    .\.venv\Scripts\activate
    ```
    
4. **Instale as dependências:**
    
    ```bash
    
    python -m pip install --upgrade pip
    python -m pip install -r requirements.txt
    ```
    
5. **Rode a dashboard:**
    
    ```bash
    
    python -m streamlit run src/dashboard.py
    ```
    
    - Acesse [http://localhost:8501](http://localhost:8501/) no navegador

---

## 🧠 **FAQ Rápido**

- **Preciso rodar sensores ou API local?**
    
    ❌ Não, só a dashboard localmente (ou use a da nuvem).
    
- **Consigo ver os alertas sendo salvos?**
    
    ✅ Sim, acesse a API Central no link abaixo para visualizar.
    
- **Posso trocar pra sensores reais no futuro?**
    
    ✅ Sim, basta publicar os dados no MQTT nos mesmos tópicos.
    
- **Erros ao instalar dependências?**
    
    Verifique se está usando **Python 3.11** e siga os passos acima.
    
    ---
    

## 🌩️ **Serviços em Produção (Nuvem)**

- **API Central (.NET):**
    - [API Central (.NET) - Visualizar alertas](https://disasterlink-api.fly.dev/api/alertasclimaticos)
    - [API de Anomalia (FastAPI)](https://anomaly-api.fly.dev/docs#/)
    - [Dashboard em Nuvem (Testes)](https://disasterlink-dashboard.fly.dev)

---

## 👥 **Equipe de Desenvolvimento**

[](data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==)

**Macauly Vivaldo da Silva** — RM: 553350 | 2TDSPC

**Daniel Bezerra da Silva Melo** — RM: 553792 | 2TDSPC

**Gustavo Rocha Caxias** — RM: 553310 | 2TDSPA

---

[](data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==)

[](data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==)
