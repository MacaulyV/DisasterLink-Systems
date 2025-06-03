# Arquitetura de Pipeline CI/CD – DisasterLink Systems

![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/155f81d4-12a3-4e55-9f6e-6b7c8041b3f8)

## 🧑‍💻 Equipe de Desenvolvimento

- **Macauly Vivaldo da Silva** — RM: 553350 | 2TDSPC
- **Daniel Bezerra da Silva Melo** — RM: 553792 | 2TDSPC
- **Gustavo Rocha Caxias** — RM: 553310 | 2TDSPA

## 🌎 **Descrição Detalhada do Projeto DisasterLink Systems**

O **DisasterLink Systems** é uma solução digital **colaborativa e inteligente** focada em situações de emergência pós-desastres naturais — enchentes, deslizamentos, temporais, incêndios, entre outros cenários críticos que afetam cidades brasileiras.

A solução visa **integrar cidadãos, autoridades e tecnologia**, centralizando:

- Informação sobre abrigos e pontos de apoio.
- Mapeamento de doações e necessidades.
- Comunicação eficiente e automatizada via alertas.
- Recomendação personalizada baseada em IA para direcionar quem precisa de ajuda ao recurso certo.

---

## 🤝 **Lógica de Integração da Solução – Como Tudo Colabora**

### 🧩 **Arquitetura Modular e Sinérgica**

A DisasterLink é composta por **três módulos principais**, cada um com responsabilidades claras, mas todos interconectados por meio da **API central**:

---

### 🚀 **API Central (.NET C#)**

- **🧠 Papel:** Cérebro da plataforma – concentra todas as regras de negócio, segurança, autenticação JWT, gestão dos dados, controle de permissões e integração com IA.
- **🔌 Integração:**
    - Serve de ponte **única** para todos os demais módulos.
    - Recebe e processa todas as requisições (cadastro, login, buscas, recomendações, registros de doação, emissão de alertas, etc).
    - Exige token JWT para ações sensíveis.
    - Responsável por auditar tudo que acontece na plataforma.

---

### 📱 **Aplicativo Mobile (React Native)**

- **👤 Papel:**
    
    Principal canal para o cidadão. Permite visualizar abrigos e pontos de coleta, receber alertas, buscar recomendações, cadastrar-se e participar de doações/voluntariado.
    
- **🔗 Integração:**
    - Consome **exclusivamente** os endpoints da API central.
    - Nunca armazena dados localmente de forma persistente: tudo é validado, salvo e lido diretamente via API, garantindo sincronização e segurança.

---

### 🖥️ **Painel Web/Admin (Java Spring Boot + Thymeleaf)**

- **🛡️ Papel:**
    
    Plataforma de administração para gestores, Defesa Civil e órgãos oficiais. Garante governança, controle e auditoria dos recursos do sistema.
    
- **🔗 Integração:**
    - Também consome a API central, utilizando endpoints de consulta e gestão.
    - Possui **endpoints próprios** para autenticação e cadastro de administradores.
    - Só usuários com perfil de admin conseguem acessar recursos críticos (ex: criar/excluir abrigos e pontos de coleta, emitir alertas).
    - Toda autenticação e autorização é centralizada via JWT, mesmo para admins.

---

### 🧬 **Como tudo funciona junto?**

- **Usuário comum** (mobile) acessa a API → visualiza abrigos/pontos/alertas, realiza login, solicita recomendações.
- **Admin** (painel web) acessa a API → gerencia recursos, cadastra/exclui entidades, emite alertas, audita dados.
- **API** controla e valida todas as transações, garantindo que regras de negócio e segurança sejam cumpridas.
- **Machine Learning ([ML.NET](http://ml.net/))** é chamado pela API nos momentos de recomendação, tornando o atendimento mais inteligente.

---

### 🌟 **Benefícios da Integração**

- **🔒 Segurança Total:** Toda operação sensível só passa com autenticação e autorização.
- **⏱️ Tempo Real e Consistência:** Mobile e web consomem sempre o dado mais recente; tudo é auditado e centralizado.
- **📈 Inteligência:** Regras de negócio, recomendações e alertas são orquestrados pela API para máxima eficiência.
- **⚙️ Facilidade de manutenção:** Qualquer melhoria ou ajuste em um módulo não afeta os outros, desde que mantenha contrato com a API.
    
    ---
    

## 🏗️ **Arquitetura e Funcionamento Geral da API**

A **API central em .NET** é o núcleo de toda a solução. Ela conecta **app mobile** (usuário comum), **painel web** (administração pública), provendo todos os dados e funcionalidades essenciais. Sua arquitetura separa responsabilidades em camadas bem definidas, garantindo segurança, escalabilidade e clareza no fluxo dos dados.

## 📦 Entidades Principais

### 🔹 Usuário

- **Campos principais**: Id, Nome, Email, Senha (hash), Localidade
- **Uso**: Representa o usuário comum do sistema, com login e registro facilitado.

### 🔸 Admin

- **Campos principais**: Id, Nome, Email, Senha (hash)
- **Uso**: Exclusivo para administração, gestão de alertas e pontos críticos.

### 🏠 Abrigo Temporário

- **Campos principais**: Id, Nome, Cidade, Endereço, Capacidade, Ocupação atual, Ativo
- **Uso**: Lista e gestão de locais seguros para abrigar pessoas após desastres.

### 📍 Ponto de Coleta de Doações

- **Campos principais**: Id, Nome, Endereço, Itens aceitos, Responsável, Contato, Ativo
- **Uso**: Gestão de locais oficiais para recebimento e distribuição de doações.

### 🚨 Alerta

- **Campos principais**: Id, Título, Descrição, Cidade, Tipo, Prioridade, Ativo
- **Uso**: Comunicação imediata de riscos e informações críticas.
    
    ---
    

## 🧪 Instruções de Testes e Execução

### 🔹 **Como Testar a API**

- **Swagger UI:**
    
    Toda a API já possui exemplos de requests e respostas documentados diretamente no Swagger.
    
    Não é necessário fornecer exemplos de JSON aqui: basta acessar o endpoint `/swagger` após iniciar o projeto, onde você poderá **testar todos os recursos, enviar dados e visualizar retornos** de forma interativa e explicativa.
    

### 📑 Links Úteis para Testar e Validar o Sistema

- [🚀 **Deploy do Sistema**](https://disasterlink-api.fly.dev/swagger/index.html)
    
    Teste o sistema rodando em ambiente real, pronto pra uso.
    

---

## 🏗️ Desenho de Arquitetura CI/CD — DisasterLink

O diagrama acima traduz exatamente o fluxo pensado para o deploy e integração contínua do DisasterLink na Azure, aplicando **DevOps de verdade** e automatizando todo ciclo — do commit ao deploy em produção.

---

![image.png](https://github.com/user-attachments/assets/b7b71a24-3b8d-467a-bcf6-db14cc15754b)

---

### 🛠️ **Source (Git + C#)**

O desenvolvimento é todo versionado via Git, no repositório principal do projeto.

Qualquer alteração relevante no código C# já dispara a pipeline YAML, então não existe gap entre desenvolvimento e integração.

---

### 🚀 **Pipeline (Azure DevOps)**

Aqui está o centro do DevOps:

Tudo gira em torno da pipeline, **definida 100% como código** via YAML.

- **Nada de etapas manuais**: build, testes (quando aplicável), docker build, deploy — tudo no pipeline.
- **Secrets, variáveis de ambiente e connection strings**? São gerenciados direto nas configurações do pipeline, nunca hardcoded no código.

---

### 📄 **YAML — O “Script Mestre”**

O YAML não é só um arquivo de configuração; ele é a **documentação viva** e automatização do processo inteiro:

- **Triggers**: define quais branches ou eventos (push/PR) vão disparar a pipeline.
- **Jobs/tasks**: descreve tudo que acontece — do restore ao deploy.
- **Deploy sem gambiarra**: cada comando está ali por um motivo, do checkout ao push da imagem.
- **Portabilidade e rastreabilidade**: qualquer dev pega o repositório e entende na hora o que está rolando no CI/CD.

Resumindo:

**YAML = pipeline transparente, auditável e reprodutível, sem dependência de configuração manual na interface.**

---

### 🐳 **Docker Image**

Nada de ambiente “diferente do meu”.

O build gera uma imagem Docker padronizada, rodando .NET exatamente como esperado.

Se for usar ACR, o push está integrado.

Se não, a imagem já vai direto para o deploy.

---

### ☁️ **Azure App Service (Deploy automatizado)**

A pipeline faz o deploy da imagem no **Azure App Service (Container Linux)**, que já está configurado para puxar a imagem e injetar as configs de ambiente/connection string.

- **Zero intervenção manual**: terminou a pipeline, está em produção.
- **Escalabilidade garantida pelo Azure**, sem se preocupar com infraestrutura.

---

### 🗄️ **Azure SQL Database (Consumo apenas)**

O banco já existe no Azure (não é criado via pipeline).

A aplicação só **consome** o banco — conexão feita via connection string, passando pela camada segura de variáveis do pipeline.

Assim, o fluxo respeita a separação entre aplicação e dados, mantendo segurança e boas práticas.

---

## 🔄 **Fluxo resumido**

1. **Commit/push no Git →** dispara a pipeline via YAML.
2. **Pipeline:** Build, dockerização, (push pro ACR se usado), deploy automatizado.
3. **Deploy:** App Service sobe a imagem já com configs e aponta para o Azure SQL.
4. **Execução:** API está rodando, pronta para atender (sem “mão” em servidor).
