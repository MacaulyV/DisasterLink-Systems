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
    public class AbrigosTemporariosControllerTests
    {
        // Cliente para endpoints de AbrigosTemporarios
        private readonly HttpClient _client;
        // Cliente para endpoints de Admin (para obter JWT)
        private readonly HttpClient _adminClient;

        public AbrigosTemporariosControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/AbrigosTemporarios/")
            };
            _adminClient = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        // =====================================
        // Helper CORRIGIDO: obtém token de Admin
        // =====================================
        private async Task<string> ObterTokenAdminAsync()
        {
            var loginDto = new
            {
                email = "luciana@admin.com",
                senha = "985214"
            };

            var loginResponse = await _adminClient.PostAsJsonAsync("login", loginDto);
            loginResponse.EnsureSuccessStatusCode();

            var jsonElement = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            if (jsonElement.TryGetProperty("token", out JsonElement tokenElement))
            {
                return tokenElement.GetString()
                    ?? throw new InvalidOperationException("Token retornado veio nulo.");
            }

            throw new InvalidOperationException("Propriedade 'token' não encontrada no JSON de login.");
        }

        // ============================================
        // 1) GET /api/AbrigosTemporarios (público)
        // ============================================
        [Fact(DisplayName = "GetAll Abrigos: público retorna 200 e array")]
        public async Task GetAllAbrigos_DeveRetornar200EArray()
        {
            var resp = await _client.GetAsync("");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // ================================================================
        // 2) GET /api/AbrigosTemporarios/cidade/municipio?nomeCidade={x} (público)
        // ================================================================
        [Fact(DisplayName = "GetByCity Abrigos: sem resultados retorna 200 e lista vazia")]
        public async Task GetAbrigosByCity_NenhumEncontrado_DeveRetornar200EVazio()
        {
            var resp = await _client.GetAsync("cidade/municipio?nomeCidade=CidadeQueNaoExiste");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
            Assert.Empty(json.EnumerateArray());
        }

        // ================================================
        // 3) POST /api/AbrigosTemporarios (requer JWT)
        // ================================================
        [Fact(DisplayName = "Create Abrigo: sem JWT retorna 401")]
        public async Task CreateAbrigo_SemJwt_DeveRetornar401()
        {
            var dto = new
            {
                nome = "Abrigo Teste",
                descricao = "Descrição do abrigo",
                cidadeMunicipio = "CidadeTeste",
                bairro = "BairroTeste",
                logradouro = "Rua Exemplo, 123",
                capacidade = 50,
                imagemUrls = new List<string> { "https://exemplo.com/img1.jpg" }
            };

            var resp = await _client.PostAsJsonAsync("", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Create Abrigo: com JWT e dados inválidos retorna 400")]
        public async Task CreateAbrigo_ComJwt_DadosInvalidos_DeveRetornar400()
        {
            // 1) Obter token de admin
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 2) DTO omitindo campos obrigatórios (por exemplo, sem 'bairro' ou sem 'descricao')
            var dto = new
            {
                // nome omitido → falha de validação
                cidadeMunicipio = "CidadeTeste",
                logradouro = "Rua Teste, 456",
                capacidade = 20,
                imagemUrls = new List<string>() // mas faltam nome, descricao, bairro → 400
            };

            var resp = await clientAut.PostAsJsonAsync("", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "Create Abrigo: com JWT e dados válidos retorna 201")]
        public async Task CreateAbrigo_ComJwt_DeveRetornar201()
        {
            // 1) Obter token de admin
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 2) DTO completo, conforme a definição exata do AbrigoTemporarioCreateDto
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new
            {
                nome = $"Abrigo Teste {unique}",
                descricao = "Abrigo emergencial instalado após chuvas fortes.",
                cidadeMunicipio = "São Carlos",
                bairro = "Vila Aurora",
                logradouro = "Avenida Principal, 1234 - Ginásio Poliesportivo",
                capacidade = 120,
                imagemUrls = new List<string>
                {
                    "https://cdn.disasterlink.com.br/abrigos/vila_aurora_1.jpg"
                }
            };

            var resp = await clientAut.PostAsJsonAsync("", dto);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("id", out JsonElement idElem));
            Assert.True(idElem.GetInt32() > 0);
        }

        // ========================================================
        // 4) GET /api/AbrigosTemporarios/{id} (público)
        // ========================================================
        [Fact(DisplayName = "GetById Abrigo: id inexistente retorna 404")]
        public async Task GetAbrigoById_Inexistente_DeveRetornar404()
        {
            var resp = await _client.GetAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "GetById Abrigo: cria e busca retorna 200")]
        public async Task GetAbrigoById_Existente_DeveRetornar200()
        {
            // 1) Cria um abrigo completo (com JWT)
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Abrigo Busca {unique}",
                descricao = "Teste de criação de abrigo",
                cidadeMunicipio = "São Carlos",
                bairro = "BairroTeste",
                logradouro = "Rua Local, 789",
                capacidade = 30,
                imagemUrls = new List<string>
                {
                    "https://exemplo.com/abrigo_busca.jpg"
                }
            };
            var criarResp = await clientAut.PostAsJsonAsync("", criarDto);
            criarResp.EnsureSuccessStatusCode();

            var jsonCriar = await criarResp.Content.ReadFromJsonAsync<JsonElement>();
            int idCriado = jsonCriar.GetProperty("id").GetInt32();

            // 2) Executa GET /{idCriado}
            var resp = await _client.GetAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal($"Abrigo Busca {unique}", json.GetProperty("nome").GetString());
        }

        // ======================================================
        // 5) PATCH /api/AbrigosTemporarios/{id} (requer JWT)
        // ======================================================
        [Fact(DisplayName = "Update Abrigo: sem JWT retorna 401")]
        public async Task UpdateAbrigo_SemJwt_DeveRetornar401()
        {
            var dto = new
            {
                // Payload mesmo completo, mas sem token não autentica
                nome = "Nome Novo",
                descricao = "Descrição Nova",
                cidadeMunicipio = "Nova Cidade",
                bairro = "Novo Bairro",
                logradouro = "Nova Rua, 123",
                capacidade = 100,
                imagemUrls = new List<string> { "https://exemplo.com/novo.jpg" }
            };
            var resp = await _client.PatchAsJsonAsync("3041", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Update Abrigo: com JWT e id inválido retorna 404")]
        public async Task UpdateAbrigo_ComJwt_IdInexistente_DeveRetornar404()
        {
            // 1) Obter token de admin
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 2) DTO completo (todos os campos obrigatórios) para que não retorne 400
            var dtoCompleto = new
            {
                nome = "Nome Qualquer",
                descricao = "Descrição qualquer",
                cidadeMunicipio = "CidadeTeste",
                bairro = "BairroTeste",
                logradouro = "Rua Teste, 123",
                capacidade = 50,
                imagemUrls = new List<string> { "https://exemplo.com/img.jpg" }
            };

            // 3) ID 999999 não existe → controller lançará KeyNotFoundException e retornará 404
            var resp = await clientAut.PatchAsJsonAsync("999999", dtoCompleto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "Update Abrigo: com JWT e dados inválidos retorna 400")]
        public async Task UpdateAbrigo_ComJwt_DadosInvalidos_DeveRetornar400()
        {
            // 1) Cria abrigo válido (para obter ID)
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Abrigo Update {unique}",
                descricao = "Teste de update",
                cidadeMunicipio = "CidadeTeste",
                bairro = "BairroTeste",
                logradouro = "Rua XYZ, 123",
                capacidade = 40,
                imagemUrls = new List<string> { "https://exemplo.com/update.jpg" }
            };
            var criarResp = await clientAut.PostAsJsonAsync("", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int idCriado = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Envia payload INCOMPLETO ou com campo inválido (por exemplo, capacidade negativa)
            var dtoInvalido = new
            {
                capacidade = -5, // valor inválido, e faltam outros campos obrigatórios
            };

            var resp = await clientAut.PatchAsJsonAsync(idCriado.ToString(), dtoInvalido);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "Update Abrigo: com JWT e dados válidos retorna 204")]
public async Task UpdateAbrigo_ComJwt_DadosValidos_DeveRetornar204()
{
    // 1) Obter token e criar cliente autenticado
    string token = await ObterTokenAdminAsync();
    var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
    clientAut.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // 2) Cria abrigo para teste
    var unique = Guid.NewGuid().ToString().Substring(0, 8);
    var criarDto = new
    {
        nome = $"Abrigo Atualiza {unique}",
        descricao = "Teste atualização",
        cidadeMunicipio = "São Carlos",
        bairro = "Vila Aurora",
        logradouro = "Rua ABC, 456",
        capacidade = 60,
        imagemUrls = new List<string> { "https://exemplo.com/img.jpg" }
    };
    var criarResp = await clientAut.PostAsJsonAsync("", criarDto);
    criarResp.EnsureSuccessStatusCode();
    
    // 3) Extrai o ID criado
    var jsonCriar = await criarResp.Content.ReadFromJsonAsync<JsonElement>();
    int idCriado = jsonCriar.GetProperty("id").GetInt32();

    // 4) Monta payload completo para atualização
    var dtoUpdate = new
    {
        nome = $"Abrigo Atualizado {unique}",
        descricao = "Descrição atualizada",
        cidadeMunicipio = "Nova Cidade",
        bairro = "Novo Bairro",
        logradouro = "Rua Atualizada, 789",
        capacidade = 80,
        imagemUrls = new List<string> { "https://exemplo.com/updated.jpg" }
    };

    // 5) Chama PATCH /{idCriado} — sem "EnsureSuccessStatusCode"
    var respUpdate = await clientAut.PatchAsJsonAsync(idCriado.ToString(), dtoUpdate);
    Assert.Equal(HttpStatusCode.NoContent, respUpdate.StatusCode);

    // 6) Confirma via GET que a atualização foi aplicada
    var respGet = await _client.GetAsync(idCriado.ToString());
    respGet.EnsureSuccessStatusCode();
    var json = await respGet.Content.ReadFromJsonAsync<JsonElement>();
    Assert.Equal($"Abrigo Atualizado {unique}", json.GetProperty("nome").GetString());
    Assert.Equal(80, json.GetProperty("capacidade").GetInt32());
}


        // ======================================================
        // 6) DELETE /api/AbrigosTemporarios/{id} (requer JWT)
        // ======================================================
        [Fact(DisplayName = "Delete Abrigo: sem JWT retorna 401")]
        public async Task DeleteAbrigo_SemJwt_DeveRetornar401()
        {
            var resp = await _client.DeleteAsync("3041");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Delete Abrigo: com JWT e id inválido retorna 404")]
        public async Task DeleteAbrigo_ComJwt_IdInexistente_DeveRetornar404()
        {
            // 1) Obter token e criar cliente autenticado
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.DeleteAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "Delete Abrigo: com JWT retira e confirma retorna 204")]
        public async Task DeleteAbrigo_ComJwt_DeveRetornar204()
        {
            // 1) Cria abrigo para teste
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Abrigo Deleta {unique}",
                descricao = "Teste delete",
                cidadeMunicipio = "CidadeTeste",
                bairro = "BairroTeste",
                logradouro = "Rua Delete, 789",
                capacidade = 25,
                imagemUrls = new List<string> { "https://exemplo.com/delete.jpg" }
            };
            var criarResp = await clientAut.PostAsJsonAsync("", criarDto);
            criarResp.EnsureSuccessStatusCode();
            int idCriado = (await criarResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

            // 2) Exclui
            var respDelete = await clientAut.DeleteAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.NoContent, respDelete.StatusCode);

            // 3) Confirma remoção via GET
            var respGet = await _client.GetAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.NotFound, respGet.StatusCode);
        }
    }
}
