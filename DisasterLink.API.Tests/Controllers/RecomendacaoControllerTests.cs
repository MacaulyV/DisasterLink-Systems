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
    public class RecomendacaoControllerTests
    {
        private readonly HttpClient _client;
        private readonly HttpClient _adminClient;

        public RecomendacaoControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/Recomendacao/")
            };
            _adminClient = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        /// <summary>
        /// Helper: obtém token JWT de Admin para endpoints protegidos.
        /// </summary>
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
        // 1) POST /api/Recomendacao/pontos-coleta
        // ================================================================

        [Fact(DisplayName = "ObterRecomendacoes: sem 'necessidade' retorna 400")]
        public async Task ObterRecomendacoes_SemNecessidade_Retorna400()
        {
            // envia objeto com Necessidade em branco
            var dto = new
            {
                necessidade = "",
                cidade = "São Paulo"
            };

            var resp = await _client.PostAsJsonAsync("pontos-coleta", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "ObterRecomendacoes: com parâmetros válidos retorna 200 ou 404")]
        public async Task ObterRecomendacoes_ParametrosValidos_Retorna200Ou404()
        {
            // envia objeto com Necessidade e Cidade válidos
            var dto = new
            {
                necessidade = "alimentos",
                cidade = "São Paulo"
            };

            var resp = await _client.PostAsJsonAsync("pontos-coleta", dto);

            // A API pode retornar 200 com lista (mesmo que vazia)
            // ou 404 se não encontrar nenhum ponto para esse critério
            Assert.True(
                resp.StatusCode == HttpStatusCode.OK ||
                resp.StatusCode == HttpStatusCode.NotFound
            );

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                // Se for 200, o corpo deve ser um array de objetos
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
                Assert.Equal(JsonValueKind.Array, json.ValueKind);
            }
        }

        // ====================================================================
        // 2) POST /api/Recomendacao/melhor-ponto-coleta
        // ====================================================================

        [Fact(DisplayName = "ObterMelhorRecomendacao: sem 'necessidade' retorna 400")]
        public async Task ObterMelhorRecomendacao_SemNecessidade_Retorna400()
        {
            var dto = new
            {
                necessidade = "",
                cidade = "Rio de Janeiro"
            };

            var resp = await _client.PostAsJsonAsync("melhor-ponto-coleta", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "ObterMelhorRecomendacao: com parâmetros válidos retorna 200 ou 404")]
        public async Task ObterMelhorRecomendacao_ParametrosValidos_Retorna200Ou404()
        {
            var dto = new
            {
                necessidade = "medicamentos",
                cidade = "Rio de Janeiro"
            };

            var resp = await _client.PostAsJsonAsync("melhor-ponto-coleta", dto);

            // A API pode retornar 200 com um objeto ou 404 se não houver nenhum
            Assert.True(
                resp.StatusCode == HttpStatusCode.OK ||
                resp.StatusCode == HttpStatusCode.NotFound
            );

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                // Se for 200, o corpo deve ser um objeto JSON com campos de PontoColetaRecomendadoDTO
                var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
                Assert.True(json.ValueKind == JsonValueKind.Object);
                Assert.True(json.TryGetProperty("pontoId", out _));
                Assert.True(json.TryGetProperty("score", out _));
            }
        }

        // ====================================================================
        // 3) POST /api/Recomendacao/treinar-modelo
        // ====================================================================

        [Fact(DisplayName = "TreinarModelo: sem JWT retorna 401")]
        public async Task TreinarModelo_SemJwt_Retorna401()
        {
            var resp = await _client.PostAsync("treinar-modelo", null);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "TreinarModelo: com JWT retorna 200")]
        public async Task TreinarModelo_ComJwt_Retorna200()
        {
            // Obter token e criar cliente autenticado
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Chama POST /treinar-modelo
            var resp = await clientAut.PostAsync("treinar-modelo", null);

            // Espera 200 OK e corpo com string de confirmação
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var msg = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(msg));
        }
    }
}
