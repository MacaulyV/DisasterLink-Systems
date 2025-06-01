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
    public class AlertasControllerTests
    {
        private readonly HttpClient _client;
        private readonly HttpClient _adminClient;

        public AlertasControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/alertas/")
            };
            _adminClient = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        // Helper: obtém token JWT de admin
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

        // 1) GET /api/alertas/ativos (requisição sem JWT → 401, com JWT → 200 + array)
        [Fact(DisplayName = "GetAll Alertas Ativos: sem JWT retorna 401")]
        public async Task GetAtivos_SemJwt_Retorna401()
        {
            var resp = await _client.GetAsync("ativos");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "GetAll Alertas Ativos: com JWT retorna 200 e array")]
        public async Task GetAtivos_ComJwt_Retorna200EArray()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.GetAsync("ativos");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // 2) GET /api/alertas/cidade?nomeCidade={}&idUsuario={} 
        [Fact(DisplayName = "GetByCity Alertas: idUsuario <= 0 retorna 400")]
        public async Task GetByCity_IdUsuarioInvalido_Retorna400()
        {
            var resp = await _client.GetAsync("cidade?nomeCidade=Teste&idUsuario=0");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "GetByCity Alertas: usuário não existe retorna 404")]
        public async Task GetByCity_UsuarioNaoExiste_Retorna404()
        {
            // idUsuario negativo forçará 400 antes, logo escolha >0 mas inexistente
            var resp = await _client.GetAsync("cidade?nomeCidade=Teste&idUsuario=999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "GetByCity Alertas: com parâmetros válidos retorna 200 e array")]
        public async Task GetByCity_ParametrosValidos_Retorna200EArray()
        {
            // Aqui não sabemos se existem alertas, então aceitamos 200 com array (podendo estar vazio)
            var resp = await _client.GetAsync("cidade?nomeCidade=SãoCarlos&idUsuario=1972");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // 3) GET /api/alertas/tipo/{tipo} (sem JWT → 401, com JWT → 200 + array)
        [Fact(DisplayName = "GetByTipo Alertas: sem JWT retorna 401")]
        public async Task GetByTipo_SemJwt_Retorna401()
        {
            var resp = await _client.GetAsync("tipo/emergencia");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "GetByTipo Alertas: com JWT retorna 200 e array")]
        public async Task GetByTipo_ComJwt_Retorna200EArray()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.GetAsync("tipo/emergencia");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Array, json.ValueKind);
        }

        // 4) GET /api/alertas/{id} (public)
        [Fact(DisplayName = "GetAlertaPorId: id inexistente retorna 404")]
        public async Task GetAlertaPorId_Inexistente_Retorna404()
        {
            var resp = await _client.GetAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "GetAlertaPorId: id pode retornar 200 ou 404")]
        public async Task GetAlertaPorId_IdAleatorio_Retorna200Ou404()
        {
            // Como não temos ID fixo, aceitamos 200 (caso exista) ou 404
            var resp = await _client.GetAsync("1");
            Assert.True(resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NotFound);
        }

        // 5) DELETE /api/alertas/{id}?idUsuario={idUsuario} (public)
        [Fact(DisplayName = "DescartarAlerta: idUsuario <= 0 retorna 400")]
        public async Task DescartarAlerta_IdUsuarioInvalido_Retorna400()
        {
            var resp = await _client.DeleteAsync("1?idUsuario=0");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "DescartarAlerta: alerta ou usuário inexistente retorna 404")]
        public async Task DescartarAlerta_Inexistente_Retorna404()
        {
            var resp = await _client.DeleteAsync("999999?idUsuario=999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "DescartarAlerta: com parâmetros válidos retorna 204 ou 404")]
        public async Task DescartarAlerta_ParametrosValidos_Retorna204Ou404()
        {
            // Não podemos garantir que existe relação alerta->usuário, então aceitamos 204 ou 404
            var resp = await _client.DeleteAsync("1?idUsuario=1");
            Assert.True(resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.NotFound);
        }

        // 6) DELETE /api/alertas/todos (requer JWT de Admin)
        [Fact(DisplayName = "DeleteTodosAlertas: sem JWT retorna 401")]
        public async Task DeleteTodosAlertas_SemJwt_Retorna401()
        {
            var resp = await _client.DeleteAsync("todos");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "DeleteTodosAlertas: com JWT retorna 200 e message")]
        public async Task DeleteTodosAlertas_ComJwt_Retorna200EMessage()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.DeleteAsync("todos");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement msgElem));
            Assert.False(string.IsNullOrEmpty(msgElem.GetString()));
        }
    }
}
