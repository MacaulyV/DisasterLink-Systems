using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DisasterLink.API.Tests.Controllers
{
    public class PontosColetaControllerTests
    {
        private readonly HttpClient _client;
        private readonly HttpClient _adminClient;

        public PontosColetaControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/PontosColeta/")
            };
            _adminClient = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        // ----------------------------------------------------------------
        // Helper: obtém token JWT de Admin (Idêntico ao usado nos outros testes)
        // ----------------------------------------------------------------
        private async Task<string> ObterTokenAdminAsync()
        {
            var loginDto = new
            {
                email = "luciana@admin.com",
                senha = "985214"
            };

            var loginResp = await _adminClient.PostAsJsonAsync("login", loginDto);
            loginResp.EnsureSuccessStatusCode();

            var json = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            if (json.TryGetProperty("token", out JsonElement tokenElem))
                return tokenElem.GetString()!;
            throw new InvalidOperationException("Token não encontrado no JSON de login.");
        }

        // ================================================================
        // 1) GET /api/PontosColeta?cidade={}&tipo={}
        // ================================================================
        [Fact(DisplayName = "GetAll PontosColeta: sem filtros retorna 200 e array")]
        public async Task GetAllPontosColeta_SemFiltros_Retorna200EArray()
        {
            var resp = await _client.GetAsync("");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        [Fact(DisplayName = "GetAll PontosColeta: com filtros retorna 200 e array")]
        public async Task GetAllPontosColeta_ComFiltros_Retorna200EArray()
        {
            var resp = await _client.GetAsync("?cidade=Porto%20Alegre&tipo=alimentos");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // ======================================================
        // 2) GET /api/PontosColeta/todos
        // ======================================================
        [Fact(DisplayName = "GetAllSemFiltro PontosColeta: retorna 200 e array")]
        public async Task GetAllSemFiltroPontsColeta_Retorna200EArray()
        {
            var resp = await _client.GetAsync("todos");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // ================================================================
        // 3) GET /api/PontosColeta/{id}
        // ================================================================
        [Fact(DisplayName = "GetPontoColetaById: id inexistente retorna 404")]
        public async Task GetPontoColetaById_Inexistente_Retorna404()
        {
            var resp = await _client.GetAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "GetPontoColetaById: id aleatório retorna 200 ou 404")]
        public async Task GetPontoColetaById_Aleatorio_Retorna200Ou404()
        {
            var resp = await _client.GetAsync("1");
            Assert.True(
                resp.StatusCode == HttpStatusCode.OK ||
                resp.StatusCode == HttpStatusCode.NotFound
            );
        }

        // ======================================================
        // 4) POST /api/PontosColeta/criar (Admin only)
        // ======================================================
        [Fact(DisplayName = "CreatePontoColeta: sem JWT retorna 401")]
        public async Task CreatePontoColeta_SemJwt_Retorna401()
        {
            var dto = new
            {
                tipo = "Arrecadação de Alimentos Não Perecíveis",
                descricao = "Ponto de coleta para alimentos não perecíveis...",
                cidade = "Porto Alegre",
                bairro = "Centro Histórico",
                logradouro = "Praça da Matriz, 1 (Em frente à Catedral)",
                imagemUrls = new List<string>
                {
                    "https://example.com/images/ponto_coleta_alimentos_1.jpg",
                    "https://example.com/images/ponto_coleta_alimentos_2.jpg"
                },
                estoque = "cestas básicas"
            };

            var resp = await _client.PostAsJsonAsync("criar", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "CreatePontoColeta: com JWT e dados inválidos retorna 400")]
        public async Task CreatePontoColeta_ComJwt_DadosInvalidos_Retorna400()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Omite campos obrigatórios: faltam tipo, cidade, bairro, logradouro etc.
            var dto = new
            {
                descricao = "Sem informações mínimas",
                imagemUrls = new List<string>()
                // Faltam: tipo, cidade, bairro, logradouro, estoque
            };

            var resp = await clientAut.PostAsJsonAsync("criar", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "CreatePontoColeta: com JWT e dados válidos retorna 201")]
        public async Task CreatePontoColeta_ComJwt_DadosValidos_Retorna201()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new
            {
                tipo = "Arrecadação de Alimentos Não Perecíveis",
                descricao = "Ponto de coleta para alimentos não perecíveis, água potável e produtos de higiene pessoal destinados às vítimas da enchente.",
                cidade = "Porto Alegre",
                bairro = "Centro Histórico",
                logradouro = "Praça da Matriz, 1 (Em frente à Catedral)",
                imagemUrls = new List<string>
                {
                    "https://example.com/images/ponto_coleta_alimentos_1.jpg",
                    "https://example.com/images/ponto_coleta_alimentos_2.jpg"
                },
                estoque = "cestas básicas"
            };

            var resp = await clientAut.PostAsJsonAsync("criar", dto);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("id", out JsonElement idElem));
            Assert.True(idElem.GetInt32() > 0);
        }

        // ======================================================
        // 5) PATCH /api/PontosColeta/{id} (Admin only)
        // ======================================================
        [Fact(DisplayName = "UpdatePontoColeta: sem JWT retorna 401")]
        public async Task UpdatePontoColeta_SemJwt_Retorna401()
        {
            var dto = new
            {
                descricao = "Nova descrição",
                estoque = "cestas básicas atualizadas"
            };
            var resp = await _client.PatchAsJsonAsync("1", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "UpdatePontoColeta: com JWT e id inexistente retorna 404")]
        public async Task UpdatePontoColeta_ComJwt_IdInexistente_Retorna404()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new
            {
                descricao = "Atualizado sem existir",
                estoque = "novo estoque"
            };
            var resp = await clientAut.PatchAsJsonAsync("999999", dto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "UpdatePontoColeta: com JWT e dados inválidos retorna 400 ou 200")]
        public async Task UpdatePontoColeta_ComJwt_DadosInvalidos_Retorna400Ou200()
        {
            // 1) Cria um ponto válido para obter ID
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var criarDto = new
            {
                tipo = "roupas",
                descricao = "Ponto temporário de doações de roupas",
                cidade = "Porto Alegre",
                bairro = "Núcleo Bom Fim",
                logradouro = "Rua das Rosas, 150",
                imagemUrls = new List<string> { "https://example.com/images/ponto_roupas.jpg" },
                estoque = "roupas em bom estado"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int idCriado = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Payload inválido: vazio (nenhum campo para atualizar)
            var dtoInvalido = new { };
            var resp = await clientAut.PatchAsJsonAsync(idCriado.ToString(), dtoInvalido);

            // Aceitamos tanto 400 (se a API decidir recusar) quanto 200 (se ela simplesmente não mudar nada)
            Assert.True(
                resp.StatusCode == HttpStatusCode.BadRequest ||
                resp.StatusCode == HttpStatusCode.OK
            );
        }

        [Fact(DisplayName = "UpdatePontoColeta: com JWT e dados válidos retorna 200")]
        public async Task UpdatePontoColeta_ComJwt_DadosValidos_Retorna200()
        {
            // 1) Cria um ponto válido para obter ID
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var criarDto = new
            {
                tipo = "alimentos",
                descricao = "Ponto de coleta inicial para alimentos",
                cidade = "Porto Alegre",
                bairro = "Menino Deus",
                logradouro = "Av. Ipiranga, 850",
                imagemUrls = new List<string> { "https://example.com/images/initial.jpg" },
                estoque = "alimentos não perecíveis"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int idCriado = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Atualiza apenas campos opcionais (descrição e estoque)
            var dtoUpdate = new
            {
                descricao = "Descrição atualizada via teste",
                estoque = "cestas básicas"
            };
            var respUpdate = await clientAut.PatchAsJsonAsync(idCriado.ToString(), dtoUpdate);
            Assert.Equal(HttpStatusCode.OK, respUpdate.StatusCode);

            var json = await respUpdate.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Descrição atualizada via teste", json.GetProperty("descricao").GetString());
            Assert.Equal("cestas básicas", json.GetProperty("estoque").GetString());
        }

        // ===========================================================================
        //  6) POST /api/PontosColeta/{pontoColetaId}/participar?idUsuario={}
        // ===========================================================================
        [Fact(DisplayName = "AddParticipacao: pontoColetaId inválido retorna 400")]
        public async Task AddParticipacao_PontoColetaIdInvalido_Retorna400()
        {
            var dto = new
            {
                formaDeAjuda = "Doação",
                telefone = "11944444444"
            };
            var resp = await _client.PostAsJsonAsync("0/participar?idUsuario=1", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "AddParticipacao: idUsuario inválido retorna 400")]
        public async Task AddParticipacao_IdUsuarioInvalido_Retorna400()
        {
            var dto = new
            {
                formaDeAjuda = "Doação",
                telefone = "11933333333"
            };
            var resp = await _client.PostAsJsonAsync("1/participar?idUsuario=0", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "AddParticipacao: com dados inválidos retorna 400")]
        public async Task AddParticipacao_DadosInvalidos_Retorna400()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 1) Cria ponto para garantir ID existente
            var criarDto = new
            {
                tipo = "roupas",
                descricao = "Coleta de roupas",
                cidade = "Porto Alegre",
                bairro = "Núcleo Santo Antônio",
                logradouro = "Rua da Paz, 22",
                imagemUrls = new List<string> { "https://example.com/images/roupas.jpg" },
                estoque = "roupas em bom estado"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int pontoId = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Body faltando campo obrigatório 'formaDeAjuda'
            var dtoInvalido = new
            {
                telefone = "11922222222"
            };
            var resp = await clientAut.PostAsJsonAsync($"{pontoId}/participar?idUsuario=1", dtoInvalido);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "AddParticipacao: recurso não encontrado retorna 404")]
        public async Task AddParticipacao_RecursoNaoEncontrado_Retorna404()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new
            {
                formaDeAjuda = "Doação",
                telefone = "11911111111"
            };
            var resp = await clientAut.PostAsJsonAsync("999999/participar?idUsuario=999999", dto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "AddParticipacao: dados válidos retorna 201, 400 ou 404")]
        public async Task AddParticipacao_DadosValidos_Retorna201Ou400Ou404()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 1) Cria ponto para garantir ID existente
            var criarDto = new
            {
                tipo = "alimentos",
                descricao = "Coleta de alimentos",
                cidade = "Porto Alegre",
                bairro = "Núcleo Independência",
                logradouro = "Av. Independência, 200",
                imagemUrls = new List<string> { "https://example.com/images/alimentos.jpg" },
                estoque = "alimentos não perecíveis"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int pontoId = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Participação válida
            var dto = new
            {
                formaDeAjuda = "Voluntariado",
                telefone = "11900000000",
                mensagem = "Disponível aos sábados",
                contato = "Contato Extra"
            };
            var resp = await clientAut.PostAsJsonAsync($"{pontoId}/participar?idUsuario=1", dto);

            // Pode retornar 201 (Created), 400 (BadRequest, se faltar algo) ou 404 (NotFound, se usuário não existir)
            Assert.True(
                resp.StatusCode == HttpStatusCode.Created ||
                resp.StatusCode == HttpStatusCode.BadRequest ||
                resp.StatusCode == HttpStatusCode.NotFound
            );
        }

        // ================================================================
        // 7) GET /api/PontosColeta/{id}/participantes
        // ================================================================
        [Fact(DisplayName = "GetParticipantes: id inválido retorna 200 e array vazio")]
        public async Task GetParticipantes_IdInvalido_Retorna200EArray()
        {
            var resp = await _client.GetAsync("999999/participantes");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        [Fact(DisplayName = "GetParticipantes: id existente retorna 200 e array")]
        public async Task GetParticipantes_IdExistente_Retorna200EArray()
        {
            // 1) Cria ponto para garantir ID existente
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var criarDto = new
            {
                tipo = "medicamentos",
                descricao = "Coleta de medicamentos",
                cidade = "Porto Alegre",
                bairro = "Centro",
                logradouro = "Rua da Saúde, 10",
                imagemUrls = new List<string> { "https://example.com/images/medicamentos.jpg" },
                estoque = "kits básicos"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int pontoId = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            var resp = await _client.GetAsync($"{pontoId}/participantes");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // ================================================================
        // 8) DELETE /api/PontosColeta/{id} (Admin only)
        // ================================================================
        [Fact(DisplayName = "DeletePontoColeta: sem JWT retorna 401")]
        public async Task DeletePontoColeta_SemJwt_Retorna401()
        {
            var resp = await _client.DeleteAsync("1");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "DeletePontoColeta: com JWT e id inexistente retorna 404")]
        public async Task DeletePontoColeta_ComJwt_IdInexistente_Retorna404()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.DeleteAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "DeletePontoColeta: com JWT retira e confirma retorna 204")]
        public async Task DeletePontoColeta_ComJwt_Retorna204()
        {
            // 1) Cria ponto para teste
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var criarDto = new
            {
                tipo = "medicamentos",
                descricao = "Ponto temporário de coleta de medicamentos",
                cidade = "Porto Alegre",
                bairro = "Núcleo Santana",
                logradouro = "Av. Santana, 500",
                imagemUrls = new List<string> { "https://example.com/images/meds.jpg" },
                estoque = "kits básicos"
            };
            var criarResp = await clientAut.PostAsJsonAsync("criar", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int pontoId = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Exclui
            var respDelete = await clientAut.DeleteAsync(pontoId.ToString());
            Assert.Equal(HttpStatusCode.NoContent, respDelete.StatusCode);

            // 3) Confirma remoção via GET → 404
            var respGet = await _client.GetAsync(pontoId.ToString());
            Assert.Equal(HttpStatusCode.NotFound, respGet.StatusCode);
        }
    }
}
