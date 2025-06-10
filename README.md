![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/155f81d4-12a3-4e55-9f6e-6b7c8041b3f8)

# 📱🌐 DisasterLink App🔗✨

## **🧑‍💻 Equipe de Desenvolvimento**

- **Macauly Vivaldo da Silva** — RM: 553350 | 2TDSPC
- **Daniel Bezerra da Silva Melo** — RM: 553792 | 2TDSPC
- **Gustavo Rocha Caxias** — RM: 553310 | 2TDSPA
    
    ---
    

# **🌎 Descrição Detalhada da Solução DisasterLink Systems**

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

Cérebro da solução. Controla as regras de negócio, segurança (autenticação JWT), auditoria de operações, persistência de dados, integração com IA e orquestração de alertas.

**🔌 Integração:**

- Serve de ponte única entre todos os módulos.
- Recebe e processa cadastros, logins, buscas, recomendações, registros de doação, emissão de alertas, etc.
- Exige token JWT para ações sensíveis.
- Responsável por auditar tudo que acontece na plataforma e App.

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

---

### 🧬 **Como Tudo Funciona Junto**

- **Usuário comum (mobile)** acessa a API para visualizar abrigos/pontos/alertas, fazer login/cadastro, receber recomendações.
- **Admin (painel web)** acessa a API para gerenciar recursos, cadastrar entidades, emitir alertas e auditar operações.
- **Módulo IoT** opera 24/7, monitorando condições ambientais e disparando alertas automáticos sem intervenção humana, alimentando a API central.

---

## 🎬 **Veja o Projeto em Ação**

Agora é só assistir o vídeo de apresentação e demonstração do DisasterLink App com tudo funcionando:

- **🔗 Link do vídeo:**
    
    [**Assista agora no YouTube**](https://youtu.be/EUSpr6TeYac)
    

---

## 🌟 **Funcionalidades Principais do DisasterLink App**

---

| 🚀 | **Inicialização Imediata**

| **Acesso instantâneo:** O app carrega rápido e já mostra uma tela de abertura limpa (Splash).

---

| 👋 | **Guia Interativo de Primeiro Acesso**

| **Onboarding imersivo:** Assim que instala, você passa por um guia visual que explica, na prática, como usar cada ferramenta essencial do DisasterLink.

---

| 📝🔑👤 | **Conta Segura & Perfil Centralizado**

| **Login e Cadastro práticos:** Você cria sua conta de forma segura e acessa um perfil central onde gerencia seus dados, histórico e preferências. No mundo real, isso permite que o app personalize alertas e recomendações pra sua situação (tipo, saber onde você está e se precisa de abrigo). |

---

| 🗺️🏕️ | Localização **de Abrigos Temporários**

| **Localização salva vidas:** Veja abrigos disponíveis no app, com detalhes de endereço, capacidade e recursos (água, comida, suporte).

---

| 📦 | **Pontos de Coleta de Doações**

| **Doação eficiente:** Descubra onde doar ou buscar suprimentos na sua região. O app mostra o que falta em cada ponto de coleta, pra sua ajuda ser certeira.

---

| ⚠️ | **Alertas Inteligentes e Notificações em Tempo Real**

| **Receba avisos essenciais:** o DisasterLink envia alertas de desastres (chuva forte, enchente) de fontes oficiais. A informação chega na hora certa, para você decidir rápido e se proteger de verdade — tudo via geolocalização.

---

| 🤖🧠 | **Assistente Virtual com IA**

| **Receba Ajuda 24h:** Nosso modelo de IA analisa dados como localização, tipo de necessidade e situação dos pontos de ajuda para recomendar, de forma inteligente, o melhor local para você ir — rápido, eficiente e personalizado. 

---

| 👤 | **Gestão Completa de Perfil**

| Tudo sobre você num lugar só: edite senha, veja seus dados, altere a foto de perfil ou capa.

---

**No mundo real:**
> 
> 
> O DisasterLink conecta cidadãos, doadores, abrigos e autoridades de forma rápida e eficiente. Cada funcionalidade existe pra resolver um problema real que acontece em desastres — seja informar, proteger, direcionar pra abrigo ou facilitar doações.
> 

---

## **🏗 Estrutura do Projeto**

```markdown
```sh
/
├── assets/                  # 🖼️ Recursos estáticos (imagens, animações)
│   ├── images/
│   └── animations/
│
├── app/                     # 📂 Código-fonte principal da aplicação
│   ├── components/          # 🧩 Componentes reutilizáveis (Footer, SideDrawer)
│   │   ├── AI/
│   │   ├── onboarding/
│   │   └── splash/
│   │
│   ├── navigation/          # 🧭 Lógica de navegação e rotas
│   │   ├── AppNavigator.tsx # - Arquivo principal que define o Stack Navigator
│   │   └── types.ts         # - Tipagem para as rotas e parâmetros
│   │
│   ├── screens/             # 📱 Telas da aplicação (1 arquivo = 1 tela)
│   │   ├── Splash.tsx
│   │   ├── Onboarding.tsx
│   │   ├── Login.tsx
│   │   ├── Register.tsx
│   │   ├── Profile.tsx
│   │   ├── AI.tsx
│   │   ├── AbrigosTemporarios.tsx
│   │   ├── PontoColeta.tsx
│   │   └── AlertasClimaticos.tsx
│   │
│   ├── services/            # ⚙️ Lógica de negócios e comunicação externa
│   │   ├── ApiService.ts    # - Funções para interagir com a API backend
│   │   ├── LocationService.ts# - Funções para geolocalização e GPS
│   │   └── OnboardingService.ts# - Lógica para o fluxo de primeiro acesso
│   │
│   └── utils/               # 🛠️ Funções auxiliares e helpers
│       └── splashHelper.ts  # - Lógica de controle da Splash Screen
│
├── App.tsx                  # ✨ Ponto de entrada principal do React Native
├── app.json                 # ⚙️ Configurações do projeto Expo
├── eas.json                 # 🏗️ Configurações do Expo Application Services (EAS)
├── package.json             # 📦 Lista de dependências e scripts
└── tsconfig.json            # 📜 Configurações do compilador TypeScript
```
```

---

## 🗺️ **Guia Visual das Telas – DisasterLink App**

| 🧭 Etapa | 🖼️ Tela | 📱 Ícone | **O que faz na prática?** |
| --- | --- | --- | --- |
| **Inicialização** | Splash | 🚀 | Tela de abertura enquanto carrega o app. |
| **Boas-vindas** | Onboarding | 👋 | Explica o app e mostra as funções principais pra quem acabou de instalar. |
| **Autenticação** | Login | 🔑 | Usuário entra na conta pra acessar as funções do app. |
| **Autenticação** | Cadastro | 📝 | Tela pra criar nova conta, preenchendo dados como nome, e-mail, senha e localização. |
| **Núcleo** | Perfil | 👤 | Mostra dados do usuário, opção de editar perfil e configurações da conta. |
| **Funcionalidade** | Abrigos | 🏕️ | Lista ou mapa dos abrigos temporários disponíveis perto do usuário. |
| **Funcionalidade** | Pontos de Coleta | 📦 | Exibe locais onde é possível doar ou receber suprimentos e doações. |
| **Funcionalidade** | Alertas | ⚠️ | Mostra alertas meteorológicos, de desastres e orientações importantes da região do usuário. |
| **Funcionalidade** | IA | 🤖 | Assistente inteligente que responde dúvidas, dá dicas e ajuda na navegação do app. |

---

## 📦 Todas as **Dependências do Projeto**

| Ícone | Biblioteca | Pra que serve? | Onde aparece no app |
| --- | --- | --- | --- |
| 🎨 | **@expo/vector-icons** | Ícones prontos e populares para menus, botões, etc | App todo |
| 💾 | **@react-native-async-storage/async-storage** | Salvar dados localmente (login, configs, cache) | Autenticação, prefs |
| 📑 | **@react-navigation/native** | Navegação entre telas (core) | App todo |
| 📚 | **@react-navigation/drawer** | Menu lateral deslizante (drawer menu) | Menu lateral |
| 🗂️ | **@react-navigation/native-stack** | Fluxo de telas em pilha (push/pop) | Navegação interna |
| ⚙️ | **expo** | Base do Expo, gerenciamento do projeto | Infraestrutura |
| 📸 | **expo-camera** | Acesso à câmera (tirar fotos, escanear) | Envio de imagens |
| 📁 | **expo-file-system** | Manipular arquivos no dispositivo | Upload/download |
| 🔡 | **expo-font** | Carregar fontes personalizadas | Layout, tipografia |
| 🖼️ | **expo-image** | Imagem otimizada | Carregamento rápido |
| 🖼️📷 | **expo-image-picker** | Selecionar fotos do device (galeria/câmera) |  Perfil |
| 🌫️ | **expo-blur** | Efeito blur (desfoque visual) | Fundos/menus |
| 🌈 | **expo-linear-gradient** | Criar gradientes bonitos | Backgrounds |
| 📍 | **expo-location** | Acessar localização/GPS | Alertas, abrigos |
| 🚦 | **expo-status-bar** | Customizar a status bar | Layout geral |
| 🚀 | **expo-splash-screen** | Tela de carregamento inicial (splash) | Startup do app |
| 🖥️ | **expo-system-ui** | Muda tema do sistema (claro/escuro) | Aparência global |
| 🌐 | **expo-linking** | Abrir links externos/deep linking | Compartilhamento |
| 🌎 | **expo-web-browser** | Abre navegador externo pelo app | Acesso a links |
| 🔄 | **react-native-gesture-handler** | Gestos avançados (swipe, drag) | Navegação/menu |
| 🤸 | **react-native-reanimated** | Animações fluídas e performáticas | Transições |
| 🟩 | **react-native-screens** | Melhora performance das telas | Navegação |
| 🛡️ | **react-native-safe-area-context** | Garante conteúdo dentro das áreas seguras | Layout notch/borda |
| 🖼️ | **react-native-gif** | Exibe GIFs animados | Feedback/layout |
| 🎬 | **lottie-react-native** | Animações Lottie (.json, ex: loaders animados) | Telas de loading |
| 📱 | **react-native** | Core do React Native (apps mobile) | Base do app |
| 🌐 | **react-dom** / **react-native-web** | Suporte para rodar no navegador (web) | Versão web |

---

## 🚀 Como Rodar Localmente

```
# Pré‑requisitos
npm i -g expo-cli   # Node >= 16

# 1. Clone o repositorio
git clone -b feature/disasterlink-mobile https://github.com/MacaulyV/DisasterLink-Systems.git

cd DisasterLink-App

# 2. Instale dependências
npm install

# 3. Rode o projeto
npx expo start        # Pressione "a" para Android ou "i" para iOS
```

---

**🧪  Como Testar a API**

- **Swagger UI:**
    
    Toda a API já possui exemplos de requests e respostas documentados diretamente no Swagger.
    
    - **Links rápidos:**
        - 📱 Baixar APK**:** [DisasterLink.App](https://foodbridge.app/)
        - 🌐 Testes a **API C# (Swagger):** [https://disasterlink-api.fly.dev/swagger](https://disasterlink-api.fly.dev/swagger/)

##
