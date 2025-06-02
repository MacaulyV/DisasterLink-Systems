# Documentação Técnica Completa – DisasterLink API

![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/155f81d4-12a3-4e55-9f6e-6b7c8041b3f8)

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
- **Machine Learning (ML.NET)** é chamado pela API nos momentos de recomendação, tornando o atendimento mais inteligente.

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
    

---

## 🧩 **Componentes e Endpoints Principais da API**

📚 **Abaixo, detalho o papel de cada grande grupo de endpoints:**

---

### 🛡️ **Admins**

- **Propósito:** Controlar, gerenciar e proteger todas as operações administrativas do sistema. Os administradores possuem privilégios especiais para manter a ordem, cadastrar novos recursos, emitir alertas, controlar dados e gerenciar usuários. Essa camada é fundamental para garantir a segurança, integridade e governança da plataforma.
- **Endpoints:**
    - `POST /api/admin/cadastrar`
        
        Por segurança, **apenas administradores já existentes** podem criar contas para novos admins. Assim atribuindo as permissões necessárias pra outra pessoa acessar e gerenciar as funcionalidades restritas da plataforma.
        
    - `POST /api/admin/login`
        
        Realiza a autenticação de um administrador e retorna um token JWT exclusivo para acesso às rotas protegidas.
        
    - `GET /api/admin/{id}`
        
        Busca um administrador específico pelo seu identificador único (ID), permitindo consulta detalhada do perfil.
        
    - `DELETE /api/admin/{id}`
        
        Remove permanentemente um administrador do sistema, revogando todos os acessos imediatamente. *Restrito a admins superiores.*
        
    - `GET /api/admin`
        
        Lista todos os administradores cadastrados no sistema, exibindo nomes, e-mails e status de cada um.
        
    - `PATCH /api/admin/{id}/editar`
        
        Atualiza os dados de um administrador (nome, e senha), garantindo atualização de perfil.
        
    - `POST /api/admin/esqueceu-senha`
        
        Endpoint para iniciar o fluxo de redefinição de senha do administrador, com validação segura para evitar acessos indevidos.
        
- **Lógica:** A autenticação de administradores é rigorosa e baseada em JWT, garantindo que apenas eles acessem funções críticas do sistema.

---

### 👤 **Usuários**

- **Propósito:** Gerenciar o ciclo de vida do usuário comum (cidadão) na plataforma. Permitir cadastro, autenticação segura, atualização e exclusão de perfil, recuperação de senha e integração com login Google/OAuth. Usuários são a base de toda a colaboração, interação e resposta rápida em cenários de emergência.
- **Endpoints:**
    - `POST /api/usuarios/cadastrar`
        
        Cadastra um novo usuário comum ultilizando o (**App**), solicitando informações obrigatórias como nome, e-mail e senha (com confirmação).
        
    - `POST /api/usuarios/login`
        
        Criado para realiza login do usuário, pelo o (**App**)
        
    - `GET /api/usuarios/{id}`
        
        Consulta detalhada do perfil de um usuário específico pelo seu identificador único (ID).
        
    - `PATCH /api/usuarios/{id}`
        
        Atualiza os dados cadastrais de um usuário existente, permitindo correção de nome, senha e dados de localização.
        
    - `DELETE /api/usuarios/{id}`
        
        Remove permanentemente um usuário do sistema pelo seu ID.
        
    - `GET /api/usuarios`
        
        Lista todos os usuários cadastrados no (**App**).
        
    - `POST /api/usuarios/esqueceu-senha`
        
        Inicia o fluxo de redefinição de senha, usando um e-mail cadastrado valido.
        
    - `POST /api/usuarios/auth/google-login`
        
        Permite autenticação do usuário via Google OAuth para o (**App**), facilitando o login e integrando localização geográfica quando permitido.
        
    - `GET /api/usuarios/localidade`
        
        Esse endpoint foi criado pra testar a captura da localização do usuário logo após o login com Google. ele recebe o nome, email e a localização do usuário, e salva tudo junto no sistema.
        
- **Lógica:** Mantém a experiência do usuário simples, rápida e segura, com validação forte de dados (e-mail único, senha forte, confirmação de senha).

---

### 🏠 **Abrigos Temporários**

- **Propósito:** Gerenciar locais seguros para acolhimento de pessoas afetadas por desastres, mantendo informações atualizadas sobre localização, capacidade, e disponibilidade. Abrigos são elementos-chave na logística de resposta e proteção à população.
- **Endpoints:**
    - `GET /api/AbrigosTemporarios`
        
        Lista todos os abrigos temporários cadastrados no sistema, mostrando nome, endereço, cidade, capacidade total, ocupação atual e status.
        
    - `POST /api/AbrigosTemporarios`
        
        Cadastra um novo abrigo temporário, informando todos os dados necessários para funcionamento (endereço, capacidade, responsável, etc). **Restrito a administradores.**
        
    - `GET /api/AbrigosTemporarios/cidade/municipio`
        
        Retorna uma lista de abrigos localizados em uma **cidade específica**, permitindo ao usuário filtrar opções próximas.
        
    - `GET /api/AbrigosTemporarios/{id}`
        
        Busca e retorna os detalhes completos de um abrigo temporário específico, a partir de seu identificador único (ID).
        
    - `PATCH /api/AbrigosTemporarios/{id}`
        
        Atualiza os dados de um abrigo existente no sistema, como capacidade, ocupação, endereço ou status de disponibilidade. **Restrito a administradores.**
        
    - `DELETE /api/AbrigosTemporarios/{id}`
        
        Remove permanentemente um abrigo temporário do sistema, bloqueando novas visualizações ou encaminhamentos para o local. **Restrito a administradores.**
        
- **Lógica:** Esse módulo garante total transparência e rastreabilidade do ciclo de doações, aproxima doadores e voluntários das demandas reais, e permite ao sistema (por localização) sugerir locais a emergenciais. Quando registrado dispara notificações automáticas como alertas para notificar os usuários.

---

### 📍 **Pontos de Coleta de Doações**

- **Propósito:** Centralizar a coleta e distribuição de recursos essenciais (alimentos, água, roupas, etc.) durante emergências. Cada ponto de coleta funciona como elo entre doadores, voluntários e quem mais precisa, permitindo rastreabilidade, organização e priorização baseada em dados.
- **Endpoints:**
    - `GET /api/PontosColeta`
        
        Lista todos os pontos de coleta **ativos** no sistema, com filtragem por cidade ou tipo.
        
    - `GET /api/PontosColeta/todos`
        
        Lista todos os pontos de coleta cadastrados, (visão administrativa).
        
    - `GET /api/PontosColeta/{id}`
        
        Busca um ponto de coleta específico pelo ID, retornando detalhes como endereço, itens disponíveis e status.
        
    - `PATCH /api/PontosColeta/{id}`
        
        Atualiza parcialmente os dados de um ponto de coleta existente (exemplo: capacidade, endereço, status de funcionamento). **Restrito a administradores.**
        
    - `DELETE /api/PontosColeta/{id}`
        
        Remove permanentemente um ponto de coleta do sistema. **Restrito a administradores.**
        
    - `POST /api/PontosColeta/criar`
        
        Cria um novo ponto de coleta, informando localização, itens disponíveis e informações de contato. **Restrito a administradores.**
        
    - `POST /api/PontosColeta/{pontoColetaId}/participar`
        
        Registra a participação de um usuário como doador ou voluntário em um ponto de coleta de doações. Associa o usuário ao ponto e ao tipo de ajuda oferecida.
        
    - `GET /api/PontosColeta/{id}/participantes`
        
        Lista todos os participantes (usuários) registrados em um ponto de coleta específico, mostrando nomes, contatos e tipo de participação.
        

---

### 🚨 **Alertas**

- **Propósito:** Os usuários do (App) recebem notificações rápidas e direcionadas sempre que um ponto de coleta ou abrigo próximo à sua localização for registrado pelos admins. Isso garante que a informação chegue na hora certa pra quem realmente precisa.
- **Endpoints:**
    - `GET /api/Alertas/ativos`
        
        Lista todos os alertas ativos no sistema, **Restrito a administradores.** exibindo avisos importantes para todos os usuários.
        
    - `GET /api/Alertas/cidade`
        
        Busca alertas filtrando por uma **cidade específica**, permitindo notificações geolocalizadas e comunicação contextual para os usuários do (**App**).
        
    - `GET /api/Alertas/tipo/{tipo}`
        
        Permite que os Admin filtrar e visualiza alertas por tipo de emergência (exemplo: ponto de coleta ou abrigo temporário).
        
    - `GET /api/Alertas/{id}`
        
        Busca um alerta específico pelo seu identificador único (**ID**), retornando todos os detalhes dele para visualização.
        
    - `DELETE /api/Alertas/{id}`
        
        Permite que o usuário exclua um alerta específico pra ele mesmo. Quando o usuário usa o app pra descartar um alerta, o app envia o ID do usuário + o ID do alerta pra API.
        
        A API registra isso na tabela (**VisualizacoesAlerta**) assim, esse alerta não aparece mais pra esse usuário nas próximas visualizações.
        
    - `DELETE /api/Alertas/todos`
        
        Remove permanentemente todos os alertas do sistema para todos os usuários. **Restrito a administradores.** Usado em grandes atualizações, redefinições de contexto ou limpeza pós-evento.
        
- **Lógica:** Permite que a Defesa Civil e os administradores ajam rápido e com precisão, gerando alertas automáticos para os usuários sempre que um ponto de coleta ou abrigo próximo à localização deles for registrado após um desastre.

---

### 🤖 **Recomendação (ML.NET)**

- **Propósito:** Ajuda usuários afetados a encontrar, de forma inteligente, o ponto de coleta mais adequado para sua necessidade. o modelo integrado ao sistema analisa múltiplos fatores — localização, perfil da cidade, tipos de itens disponíveis e características de cada ponto — e usa Machine Learning para sugerir o local ideal de forma personalizada e eficiente.
- **Endpoints:**
    - `POST /api/Recomendacao/pontos-coleta`
        
        Recebe dados do usuário (cidade, tipo de item desejado, etc.) e retorna uma **lista ordenada de pontos de coleta recomendados**, priorizando os que têm maior chance de atender à necessidade informada.
        
        *Ideal para o app mostrar sugestões ao usuário antes de ele sair de casa ou buscar ajuda.*
        
    - `POST /api/Recomendacao/melhor-ponto-coleta`
        
        Retorna o **melhor ponto de coleta** para aquela necessidade, já considerando estoque, histórico, localização e perfil da demanda.
        
        *Útil para recomendações rápidas (“Vá diretamente ao Ponto X para buscar água”).*
        
    - `POST /api/Recomendacao/treinar-modelo`
        
        Esse endpoint serve pra re-treinar o modelo de Machine Learning (ML.NET) usando todos os dados atuais do sistema. Normalmente, só administradores têm acesso a essa função, já que ela atualiza o modelo com novos dados cadastrados e garante que as recomendações continuem precisas e atualizadas.
        
    
    ---
    
    ## 🧠 **Como funciona o modelo e o fluxo de recomendação**
    
    ### **Pipeline de Treinamento**
    
    O modelo ML.NET é treinado a partir de dados reais dos pontos de coleta, O pipeline:
    
    1. **Coleta os dados relevantes:**
        - Cidade/região
        - Tipo de item necessário
    2. **Pré-processamento:**
        - Dados passam por limpeza, normalização, encoding de categorias e balanceamento das classes, para evitar recomendações enviesadas.
    3. **Treinamento supervisionado:**
        - Algoritmo de classificação/regressão aprende a priorizar pontos de coleta com base nos padrões históricos de sucesso.
    4. **Validação e exportação:**
        - O modelo é avaliado por métricas como acurácia e recall, salvo em arquivo `.zip` e carregado pela API para uso em produção.
    
    ### 🔗 **Consumo da Recomendação na API**
    
    - **Como funciona:**
        
        Sempre que um usuário solicita uma recomendação, a API envia o contexto (necessidade e localização) pro modelo de IA, que retorna:
        
        - Uma lista dos pontos de coleta mais prováveis de atender a necessidade
        - O melhor ponto absoluto sugerido
        - O score de confiança da predição
    
    ---
    
    ### ⚙️ **Treinamento e Re-treinamento do Modelo**
    
    - O endpoint `/treinar-modelo` permite que apenas *admins* atualizem o modelo de IA quando houver muitos dados novos.
    - O pipeline **nunca re-treina automaticamente** a cada requisição (isso seria pesado e poderia dar inconsistências).
    - O re-treinamento é assíncrono — pode levar alguns segundos/minutos, dependendo do volume de dados.
    - O modelo antigo **só é substituído** após validação do novo modelo treinado.
    
    ---
    
    ### 🗂️ **Inicialização Automática do Modelo**
    
    - Sempre que a API é carregada (local ou deploy), ela verifica se já existe um modelo salvo em `bin\Debug\net9.0\MLModels\ModeloPontoColeta.zip`.
    - Se não existir, ela **treina o modelo automaticamente** com os dados atuais de pontos de coleta cadastrados.
    
    ---
    
    ### 🤖 **Eficiência e Limitações do Modelo**
    
    - O modelo recomenda pontos de coleta tanto por categoria (ex: *alimentos*, *medicamentos*, etc) quanto por item específico, e funciona melhor quanto mais específico o usuário for ao informar a necessidade.
    - **Prioridade:** Sempre tenta recomendar um ponto de coleta próximo à localização do usuário, considerando isso como o principal critério.
    - **Limitações:**
        1. **Formatação:** Se o item foi salvo como "água" (com acento) e o usuário digitar "agua" (sem acento), o modelo pode não reconhecer.
        2. **Múltiplos Itens:** Se o usuário especificar mais de um item (ex: “água e roupas”), o modelo pode se confundir e recomendar errado — pois foi treinado com dados categóricos (um item/categoria por registro), e não com combinações.
    
    ---
    
    ### 💡 **Resumo prático:**
    
    O modelo de IA é eficiente pra recomendações pontuais e categorizadas, mas ainda apresenta limitações quando a necessidade do usuário envolve múltiplos itens ao mesmo tempo ou variações de formatação.
    
    ### **Garantias e lógica de negócio**
    
    - O modelo **não pode ser sobrescrito por qualquer usuário**: há proteção por perfil de acesso.
    - O modelo é desacoplado da lógica CRUD, ou seja, a API segue funcionando mesmo que o modelo precise ser atualizado ou substituído.

---

<details open>
  <summary align="center">🗂️ Estrutura Completa e Detalhada do Projeto</summary>

A arquitetura da API DisasterLink foi desenhada seguindo as melhores práticas de desenvolvimento em .NET, com forte separação de responsabilidades, extensibilidade e clareza. Cada pasta e arquivo tem papel fundamental para a organização, manutenibilidade e escalabilidade da solução.

---

### 📁 **wwwroot/**

Contém recursos estáticos, como imagens e arquivos de customização da interface Swagger.

- 🖼️ **DisasterLink-Systems-Logo.png**: Logotipo da plataforma, exibido na documentação Swagger para reforçar a identidade visual.
- 🗂️ **swagger-custom.js** e 🎨 **swagger-ui.css**: Customizam o visual e comportamento do Swagger UI, tornando a documentação mais amigável e alinhada ao branding do projeto.

---

### 📁 **Config/**

Agrupa todas as configurações da infraestrutura de documentação, centralizando o controle do Swagger:

- ⚙️ **SwaggerConfig.cs**: Define a configuração principal do Swagger, incluindo segurança, título, descrição e versionamento.
- ⚙️ **SwaggerDefaultValues.cs**: Garante que valores-padrão e exemplos sejam apresentados automaticamente na interface do Swagger.
- ⚙️ **SwaggerTagsSorter.cs**: Organiza os endpoints por tags, facilitando a navegação e compreensão dos recursos.

---

### 📁 **Controllers/**

O “cérebro” do recebimento das requisições HTTP. Cada controller representa um grupo lógico de funcionalidades da plataforma:

- 👥 **UsuariosController.cs**: Gerencia o cadastro, autenticação, edição e consulta de usuários comuns.
- 🛡️ **AdminsController.cs**: Administra o ciclo de vida dos usuários administradores, incluindo autenticação e redefinição de senha.
- 🏠 **AbrigosTemporariosController.cs**: CRUD de abrigos temporários, utilizados em cenários pós-desastre para realocação da população.
- 📍 **PontosColetaController.cs**: Controle dos pontos oficiais de coleta de doações, suas participações e voluntariado.
- 🚨 **AlertasController.cs**: Emite, filtra e exclui alertas de emergência enviados a usuários e administradores.
- 🤖 **RecomendacaoController.cs**: Centraliza as chamadas para recomendação inteligente com ML.NET.

Cada controller valida as entradas, chama a camada de serviços e retorna respostas padronizadas, sempre protegidas por autenticação JWT quando necessário.

---

### 📁 **Data/**

A base de dados do sistema, sua configuração e scripts de carga inicial:

- 🌱 **DataSeeder.cs**: Responsável por inserir automaticamente dados de exemplo no banco (como cidades, abrigos e pontos de coleta), garantindo ambientes de testes e demonstração realistas sem intervenção manual.
- 🗄️ **DisasterLinkDbContext.cs**: Representa o contexto do Entity Framework Core, conectando entidades do C# às tabelas do banco de dados relacional.

---

### 📁 **DTOs (Data Transfer Objects)/**

Centralizam toda a validação de entrada e saída de dados entre frontend e backend. Estão divididos por propósito:

- 🔑 **Auth/**: Contém objetos para autenticação, como login Google e recuperação de senha.
- 🧰 **Common/**: Objetos amplamente usados em múltiplas operações (exemplo: LoginDto, AdminDto, UsuarioDto), além dos recursos para HATEOAS (`LinkDto`, `Resource`, etc.).
- ➕ **Create/**: DTOs usados especificamente para criação de entidades, aplicando validações como `[Required]`, tamanho de campo, formato de e-mail, etc.
- 🔄 **Update/**: Objetos para atualização parcial ou total de recursos, sempre com validações de campos.
- 📥 **Response/**: DTOs que definem exatamente o que a API devolve em cada resposta, impedindo exposição acidental de campos sensíveis.
- 🧠 **ML/**: Estruturas que trafegam informações entre a API e o modelo de machine learning.

As validações são automáticas, e erros de validação resultam em respostas 400 claras para o cliente.

---

### 📁 **Entities/**

Contém todas as representações das tabelas do banco de dados em formato orientado a objetos.

- Cada classe representa uma entidade: Usuário, Admin, Abrigo, Ponto de Coleta, Participação, Alerta, Visualização de Alerta.
- Os relacionamentos (exemplo: um ponto de coleta pode ter várias participações) estão mapeados por navigation properties, facilitando consultas complexas.

---

### 📁 **Interfaces/**

Contratos que garantem que repositórios e serviços sigam padrões e sejam facilmente substituíveis.

- 📂 **Repositories/**: Definem métodos obrigatórios para persistência de cada entidade.
- 📂 **Services/**: Especificam os métodos das regras de negócio e lógica do domínio, desacoplando controllers das implementações.

Esse padrão permite a injeção de dependência, testes unitários e futuras evoluções (ex: troca de banco).

---

### 📁 **Repositories/**

Implementações reais das interfaces de repositórios, usando o Entity Framework Core para acesso ao banco de dados.

- Exemplo: `UsuarioRepository` encapsula todas as operações CRUD sobre usuários, isolando regras SQL e consultas paginadas.

---

### 📁 **Services/**

Onde toda lógica de negócio vive: checagens adicionais, validação extra, disparo de alertas automáticos, orquestração de entidades e integração com ML.NET.

- Exemplo: O `AlertaService` pode gerar um alerta automaticamente caso um abrigo atinja 80% da capacidade, enviando notificações em tempo real.

---

### 📁 **Middleware/**

Inclui middlewares globais, como tratamento de exceções, que padronizam respostas de erro e facilitam o debugging.

---

### 📁 **Mappings/**

Perfis do AutoMapper, responsável por transformar entidades em DTOs e vice-versa.

- Permite respostas enxutas, evita ciclos de referência e facilita manutenção do código.

---

### 📁 **MLModels/**

Classe(s) de consumo do modelo treinado com ML.NET, isolando qualquer dependência da biblioteca de IA.

- Permite trocar ou atualizar modelos sem alterar os demais serviços.

---

### 📁 **SwaggerExamples/**

Arquivos que exemplificam requisições e respostas para o Swagger UI, facilitando testes e onboarding de desenvolvedores.

---

### 📁 **Utils/**

Utilitários diversos, como conversores customizados para serialização de datas, formatação e logs.

---

### 📁 **Migrations/**

Scripts gerados automaticamente para versionamento do schema do banco de dados via Entity Framework Core.

---

### **Outros arquivos**

- 🐳 **Dockerfile**: Permite deploy em containers Docker, facilitando CI/CD.
- 🛫 **fly.toml**: Configuração de deploy no Fly.io.
- 🚀 **Program.cs**: Arquivo principal do .NET 6/7, inicializa toda a aplicação, DI, middlewares, configurações globais.
- ⚙️ **appsettings.json**: Centraliza conexões de banco, parâmetros de JWT, políticas de rate limit, etc.
- 📄 **Readme.md**: Documentação de build, endpoints e instruções de uso para devs e avaliadores.
</details>

---

## ⚙️ **Destaques Técnicos, Boas Práticas & Tecnologias Utilizadas**

### 🛡️ **Autenticação e Autorização Centralizada**

- **JWT (JSON Web Token):**
    
    Todos os endpoints sensíveis exigem autenticação via token JWT.
    
    Cada token carrega as claims de perfil (`admin`, `usuario`), permitindo segregação de acessos e máxima segurança.
    
- **Políticas de Autorização:**
    
    Perfis separados para admins e usuários comuns, restringindo ações críticas somente para quem tem permissão.
    

---

### 📦 **Validações Fortes e Automatizadas**

- **DTOs com Data Annotations:**
    
    Todos os dados de entrada e saída passam por DTOs validados automaticamente (`[Required]`, `[EmailAddress]`, `[StringLength]`, etc).
    
- **Feedback Instantâneo:**
    
    Inputs inválidos retornam mensagens claras e padronizadas, prevenindo falhas e facilitando a experiência do usuário/cliente da API.
    

---

### 🛑 **Proteção Contra Abusos – Rate Limit**

- **AspNetCoreRateLimit:**
    
    Limita o número de requisições por IP (ex: 10 req/s, 100 req/min), protegendo contra ataques DoS, brute force e uso indevido.
    
- **Configuração Centralizada:**
    
    Parâmetros facilmente ajustáveis via `appsettings.json`.
    

---

### 📃 **Documentação Viva e Personalizada**

- **Swagger/OpenAPI:**
    
    Toda a API é auto-documentada com exemplos reais de request e response.
    
- **Customização Visual:**
    
    Uso de CSS e JS próprios para identidade visual da plataforma (logo, cores, organização).
    
- **SwaggerExamples:**
    
    Exibe exemplos de payload reais para todos os endpoints, facilitando testes e integração por desenvolvedores externos.
    

---

### 🪄 **Seed de Dados Automatizado**

- **DataSeeder:**
    
    Ao subir o projeto, um seed automático popula o banco com abrigos, pontos de coleta, usuários e admins de exemplo — tornando a demo pronta para testes e apresentações, sem esforço manual.
    

---

### 🔗 **HATEOAS (Hypermedia as the Engine of Application State)**

- **Endpoints Enriquecidos:**
    
    Respostas trazem links de navegação (`self`, `update`, `delete`, etc.), permitindo evolução da API para um modelo REST Full navegável.
    
- **Facilita Integração:**
    
    Ajuda frontends a descobrirem rotas e possibilidades sem hardcode.
    

---

### 🤖 **Machine Learning Real (ML.NET)**

- **Recomendações Inteligentes:**
    
    ML.NET integrado ao backend recomenda pontos de coleta para o usuário, usando dados reais e histórico.
    

---

### 🗄️ **Arquitetura e Padrões de Projeto**

- **Repository Pattern:**
    
    Isola lógica de persistência, facilita testes, garante desacoplamento.
    
- **Service Layer:**
    
    Toda regra de negócio concentrada nos serviços, separando apresentação da lógica.
    
- **AutoMapper:**
    
    Conversão automática entre entidades e DTOs, evitando repetição de código e expondo só o necessário.
    
- **Exception Middleware:**
    
    Padroniza tratamento de erros em toda a API, sempre retornando JSON amigável.
    

---

### 🚀 **Infraestrutura Moderna e DevOps**

- **Docker:**
    
    Projeto pronto para rodar em containers, garantindo ambiente replicável e deploy sem surpresas.
    
- **Deploy Cloud-Ready:**
    
    Arquivo `fly.toml` para deploy automatizado no Fly.io.
    
- **Pronto para CI/CD:**
    
    Estrutura preparada para pipelines de build, test, migrate e release em qualquer nuvem moderna.
    

---

### 🧑‍💻 **Stack Tecnológica**

- **Backend:** ASP.NET Core 9 (C#)
- **Banco de Dados:** SQL Database/Azure SQL (ORM: Entity Framework Core)
- **Documentação:** Swagger + custom CSS
- **Autenticação:** JWT
- **Machine Learning:** ML.NET
- **DevOps:** Docker, Fly.io, Azure DevOps/GitHub Actions (pronto para pipeline)
- **Testes:** xUnit (implementado, documentado à parte)
- **Frontend:** React Native (mobile), Java Spring Boot + Thymeleaf (admin)

---

## 🧪 Instruções de Testes e Execução

### 🔹 **Como Testar a API**

- **Swagger UI:**
    
    Toda a API já possui exemplos de requests e respostas documentados diretamente no Swagger.
    
    Não é necessário fornecer exemplos de JSON aqui: basta acessar o endpoint `/swagger` após iniciar o projeto, onde você poderá **testar todos os recursos, enviar dados e visualizar retornos** de forma interativa e explicativa.
    
- **Testes Automatizados (xUnit):**
    
    A documentação detalhada dos testes automatizados da API, utilizando xUnit, está disponível em um **repositório separado** para manter o conteúdo organizado, já que esta documentação 
    
    ### 📑 Links Úteis para Testar e Validar o Sistema
    
    - [🔗 **Documentação de Testes xUnit**](https://github.com/MacaulyV/DisasterLink-Systems/tree/test/api-tests)
        
        Veja como os testes automatizados foram realizados e confira os resultados.
        
    - [🚀 **Deploy do Sistema**](https://disasterlink-api.fly.dev/swagger/index.html)
        
        Teste o sistema rodando em ambiente real, pronto pra uso.
        

---

### 🚀 **Como Executar o Projeto**

1. **Clone o repositório principal da API:**
    
    ```bash
    git clone [URL_DO_SEU_REPOSITORIO]
    ```
    
2. **Instale as dependências necessárias (.NET 8+):**
    
    ```bash
    dotnet restore
    ```
    
3. **Ajuste a string de conexão no `appsettings.json` conforme seu ambiente (por padrão, já configurada para SQL Server local/nuve Azure).**
4. **Suba as migrations e popule o banco com os dados de exemplo:**
    
    ```bash
    dotnet ef database update
    ```
    
5. **Execute a aplicação:**
    
    ```bash
    dotnet run
    ```
    
6. **Acesse o Swagger UI pelo navegador:**
    
    ```
    http://localhost:PORT/swagger
    ```
    
    *(O PORT padrão é o informado no seu `launchSettings.json` ou pelo terminal na inicialização)*
    
    ---
    
    ## 🧑‍💻 Equipe de Desenvolvimento
    
    - **Macauly Vivaldo da Silva** — RM: 553350 | 2TDSPC
    - **Daniel Bezerra da Silva Melo** — RM: 553792 | 2TDSPC
    - **Gustavo Rocha Caxias** — RM: 553310 | 2TDSPA
