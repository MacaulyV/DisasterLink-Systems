# DisasterLink Systems

![DisasterLink Systems-Capa.png](DisasterLink_Systems-Capa.png)

# ⚡🌍 DisasterLink Systems — Plataforma colaborativa para mapear, monitorar e responder rapidamente a desastres naturais.

---

## 🧩 **Problema**

Desastres naturais como enchentes, deslizamentos, ondas de calor e tempestades causam impactos imediatos e severos nas cidades brasileiras.

🚫 **Desafio atual:**

- Informação descentralizada e lenta
- Moradores sem canal fácil de denúncia ou ajuda
- Defesa Civil e órgãos oficiais com pouca visibilidade em tempo real
- Dificuldade para priorizar áreas e direcionar recursos de forma eficiente
- Falta de integração entre tecnologia, população e resposta oficial

---

## 💡 **Solução Proposta**

### **Plataforma Integrada de Mapeamento Colaborativo e Monitoramento em Tempo Real**

### 🗺️ **O que é?**

Um ecossistema digital composto por aplicativo mobile, painel web, API central e integração com dispositivos IoT, onde:

- Moradores e voluntários **cadastram relatos de problemas, enviam fotos e localizações** diretamente do app ou web.
- Dados são **validados e priorizados** por autoridades/Defesa Civil via dashboard administrativo.
- Sensores IoT monitoram pontos críticos (nível de água, temperatura, presença em abrigos) e alimentam o sistema em tempo real.
- Toda a comunidade **visualiza o mapa atualizado**, recebe alertas e acompanha campanhas de prevenção e doação.

---

## 🎯 **Objetivo**

- **Centralizar, democratizar e agilizar** a comunicação e a tomada de decisão durante desastres.
- **Unir cidadãos, autoridades e tecnologia** numa resposta mais eficiente, salvando vidas e recursos.
- **Gerar histórico e inteligência** para prevenção futura de tragédias.

---

## 👥 **Público-Alvo**

- Moradores de áreas de risco
- Defesa Civil e órgãos municipais/estaduais
- Voluntários e ONGs
- Toda a comunidade afetada

---

## 🛠️ **Como Funciona?**

### **1. Relatos Colaborativos**

📱 Moradores/voluntários enviam relatos (alagamento, pedido de socorro, obstrução de via, etc.)

📸 Anexação de fotos, Vídeo e localização via GPS

🗂️ Cada relato recebe status (pendente, aprovado, resolvido)

### **2. Painel Administrativo Web**

🖥️ Dashboard para autoridades visualizarem relatos e áreas afetadas

✔️ Validação e priorização dos relatos

📊 Relatórios em tempo real para tomada de decisão

### **3. Monitoramento IoT**

🔌 Sensores em campo (nível de água, temperatura, presença) conectados via MQTT/HTTP

📈 Dados em tempo real atualizam o painel e podem disparar alertas automáticos

### **4. Alertas e Campanhas**

🚨 Alertas automáticos ou manuais para moradores próximos a áreas de risco

🤝 Campanhas de doação, prevenção e mobilização organizadas via plataforma

### **5. Histórico & Inteligência**

🕒 Registro histórico de ocorrências e alertas

🧠 Base para análises futuras e estratégias de prevenção

---

## 🏆 **Diferenciais**

- **100% colaborativo:** qualquer cidadão pode participar
- **Tempo real:** informações circulam rapidamente
- **Validação oficial:** dashboard para Defesa Civil priorizar e agir
- **Integração IoT:** sensores físicos alimentam o sistema automaticamente
- **Acessível:** web, mobile, fácil para todos os perfis de usuário
- **Escalável:** pode ser replicado para diversas cidades e estados

---

## **💻 Tecnologias e Ferramentas Utilizadas**

---

### 🧩 **.NET API & Backend Central**

- **C#:** Desenvolvimento da API principal
- **ASP.NET Core Web API:** Estrutura do backend
- **Oracle DB:** Banco de dados relacional
- **Swagger:** Documentação automática da API
- **ML.NET:** Machine Learning integrado na API
- **xUnit:** Testes automatizados
- **RESTful:** Arquitetura de API
- **HATEOAS, Rate Limit:** Boas práticas de design
- **Docker:** Containerização e deploy
- **Azure Cloud:** Hospedagem e banco na nuvem ☁️

---

### ☕ **Java MVC Web Plataform**

- **Spring MVC:** Estrutura web backend
- **Thymeleaf:** Templates dinâmicos
- **OAuth2:** Autenticação segura
- **RabbitMQ:** Produtor/consumidor de mensagens
- **Spring AI:** Recursos de IA para análise de dados
- **Testes unitários/integrados:** Cobertura de qualidade
- **Internacionalização:** Suporte a múltiplos idiomas
- **Deploy em Azure ou VM** ☁️

---

### 📱 **DisasterLink App**

- **React Native (Expo):** Desenvolvimento cross-platform
- **React Navigation:** Navegação entre telas
- **Axios/Fetch:** Comunicação com APIs
- **Firebase Auth:** Autenticação de usuários
- **Custom Design:** Identidade visual própria
- **Styled Components/** Estilização

---

### 🤖 **IoT & Dispositivos Físicos**

- **ESP32/Arduino:** Microcontroladores dos sensores
- **Sensores físicos:** Nível de água, temperatura, presença
- **Node-RED / Thinger.io / ThingSpeak:** Gateways de integração
- **MQTT / HTTP (JSON):** Protocolos de comunicação
- **Wokwi:** Simulação de hardware
- **Dashboard em Node-RED:** Painel de monitoramento em tempo real
- **Azure IoT Hub (opcional):** Gerenciamento em nuvem ☁️

---

### 🪐 **Banco de Dados Relacional e Não Relacional**

- **Oracle:** Banco principal relacional (dados estruturados)
- **PL/SQL:** Procedures, functions, triggers, packages
- **MongoDB:** Banco NoSQL para logs, relatos livres, metadados de imagens
- **Integração Oracle/MongoDB:** Sincronização de dados

---

### 🧑‍💻 **DevOps & Cloud**

- **Azure Boards:** Gestão ágil, backlog e SCRUM
- **Azure DevOps / Pipelines:** CI/CD, build, deploy automatizado
- **Docker:** Imagens e containers
- **GitHub:** Versionamento, repositório e documentação
- **Draw.io / Excalidraw / Visual Paradigm:** Diagramas de arquitetura
- **Vídeos explicativos:** Demonstração do sistema e dos deploys

---

### 📈 **Quality Assurance & Testes**

- **xUnit, JUnit:** Testes automatizados .NET e Java
- **Testes manuais:** Roteiros e validação de funcionalidades
- **Azure Boards:** Planejamento e acompanhamento dos testes
- **Critérios de aceite SCRUM:** Validação das entregas

---

## **🧑‍💻 Equipe de Desenvolvimento**

- **Macauly Vivaldo da Silva** – *Frontend & UX/UI, IA & Backend*
- **Daniel Bezerra da Silva Melo** – *Mobile Developer & Infraestrutura DevOps (Deploy)*
- **Gustavo Rocha Caxias** – *Banco de Dados*
