![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/155f81d4-12a3-4e55-9f6e-6b7c8041b3f8)

# 🧪 Documentação de Testes Automatizados — DisasterLink API

---

## 🎯 **Objetivo dos Testes Automatizados**

A suíte de testes com **xUnit** para a DisasterLink API foi criada para **garantir a robustez, confiabilidade e estabilidade** de todas as funcionalidades críticas do sistema. Os testes validam os principais fluxos de negócio, autenticação, autorização, regras de validação, integração dos endpoints e respostas adequadas a diferentes cenários de uso (válidos, inválidos e casos de erro).

---

## 🧩 **Cobertura e Organização dos Testes**

Os testes foram organizados em classes que seguem a estrutura dos controllers da API, garantindo uma separação clara e fácil manutenção:

| Controller | Quantidade de Testes | Status |
| --- | --- | --- |
| AbrigosTemporariosController | 14 | ✅ Sucesso |
| AdminsController | 11 | ✅ Sucesso |
| AlertasController | 14 | ✅ Sucesso |
| PontosColetaController | 22 | ✅ Sucesso |
| RecomendacaoController | 6 | ✅ Sucesso |
| UsuariosController | 17 | ✅ Sucesso |
| **Total** | **84** | **✅ 100%** |

✅ **Todas as execuções dos testes obtiveram sucesso, evidenciando a cobertura e confiabilidade da API. Importante destacar que todos os testes foram realizados com a API já em produção (deploy), e não apenas em ambiente local.**

---

## 🔍 **Exemplos de Casos Testados**

### Pontos de Coleta

- **Adição de participação:**
    - Com dados válidos retorna 201 ou 204
    - Com dados inválidos retorna 400
    - Sem JWT retorna 401
    - Ponto inexistente retorna 404
- **Criação, atualização e deleção de ponto:**
    - Sucesso com JWT
    - Falha sem JWT
    - Falha com ID inválido/inexistente

### Alertas

- **Busca de alertas ativos, por cidade, tipo ou ID**
- **Exclusão individual e em massa**
- **Acesso autorizado e não autorizado**

### Usuários

- **Cadastro, login e atualização de perfil**
- **Fluxo de redefinição de senha**
- **Exclusão e consulta de usuários**

### Recomendações (ML.NET)

- **Recomendações com parâmetros válidos**
- **Retorno de erro para parâmetros inválidos**
- **Re-treinamento de modelo autorizado vs. não autorizado**

---

## 📈 **Boas Práticas Aplicadas**

- **Nomenclatura clara dos métodos:**
    
    Cada teste descreve explicitamente o cenário e o resultado esperado.
    
- **Cobertura de autenticação e autorização:**
    
    Testes sempre contemplam ações com e sem JWT, além de validação de claims de perfil.
    
- **Validação de status code e payload:**
    
    Asserções para todos os códigos HTTP relevantes (200, 201, 204, 400, 401, 404, etc).
    

---

## 🖼️ **Evidências Visuais**

- ✅ Todos os testes passaram com sucesso:
      ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/522dca10-0f65-44ea-b4c0-cf86f46186d3)
  
    - **84 testes automatizados cobrindo todas as áreas críticas da API.**
- Abaixo, veja os resultados **visuais** de cada conjunto de testes automatizados, organizados por controller. As imagens comprovam que **todos os cenários críticos foram contemplados e executados com sucesso**.

---

### 🏠 **AbrigosTemporariosControllerTests**

- **Descrição:**
    
    Abrange testes de criação, atualização, deleção e consulta de abrigos, cobrindo cenários válidos, inválidos, autenticação obrigatória e respostas a erros comuns.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/d02034b3-6b15-492b-b9f4-37296308291c)

---

### 🛡️ **AdminsControllerTests**

- **Descrição:**
    
    Testa cadastro, autenticação (login), edição, exclusão, busca e redefinição de senha de administradores. Verifica comportamento para admin duplicado, dados inválidos e autorização.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/01d1619c-cc79-47b0-89ab-20061cd9df57) 

---

### 🚨 **AlertasControllerTests**

- **Descrição:**
    
    Valida listagem, consulta, exclusão individual e em massa, filtros por cidade/tipo/ID e o fluxo de descartar alerta. Testa diferentes níveis de permissão e autenticação.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png]((https://github.com/user-attachments/assets/b3da96ff-e8b2-422c-84ca-8dca398d24da)
    
---

### 📍 **PontosColetaControllerTests**

- **Descrição:**
    
    Engloba cadastro, atualização, deleção, participação em ponto, consulta por ID/filtro, cobertura de autenticação e validação de dados. Garante segurança e robustez no ciclo de vida dos pontos de coleta.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/83e8e53d-91ae-4388-8979-2057b4933041)
    
---

### 🤖 **RecomendacaoControllerTests**

- **Descrição:**
    
    Testa recomendações (ML.NET) com e sem parâmetros obrigatórios, re-treinamento do modelo, acesso com/sem JWT e respostas esperadas (200/401/404/400).
    
    Prova que a inteligência da API está protegida e validada.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/6d4d0532-7015-4e2c-b4e9-087e81038df6)
    
---

### 👤 **UsuariosControllerTests**

- **Descrição:**
    
    Valida cadastro, login, exclusão, atualização, redefinição de senha, busca por localização e fluxos de erros comuns (email duplicado, campos obrigatórios ausentes, etc).
    
    Garante a segurança e consistência do ciclo de vida do usuário.
    
- **Evidência:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/13a43779-158f-4f99-a81c-6454831237dc)

---

## 🛡️ **Qodana — Análise de Qualidade do Código**

- O projeto DisasterLink foi inspecionado com o Qodana para garantir:
    - **Ausência de erros críticos de design**
    - **Recomendações para refatoração**
- **Evidência Visual:**
    
    ![DisasterLink Systems-Capa.png](https://github.com/user-attachments/assets/2142f75e-95de-4ca6-ba3a-b62e829207c0)
    
- **O que pode ser concluído?**
    
    Mesmo com um código grande, a análise mostra que não há problemas de alta gravidade, e todos os avisos e sugestões podem ser tratados com refatoração incremental, sem comprometer o funcionamento. Isso demonstra maturidade do código e cultura de qualidade na equipe.

  ---

  ## 🧑‍💻 Equipe de Desenvolvimento
    
    - **Macauly Vivaldo da Silva** — RM: 553350 | 2TDSPC
    - **Daniel Bezerra da Silva Melo** — RM: 553792 | 2TDSPC
    - **Gustavo Rocha Caxias** — RM: 553310 | 2TDSPA
