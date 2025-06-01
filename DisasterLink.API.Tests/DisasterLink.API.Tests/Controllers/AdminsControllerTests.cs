using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DisasterLink.API.Tests.Controllers
{
    public class AdminsControllerTests
    {
        // HttpClient base para endpoints públicos (cadastrar, login, forgot-password)
        private readonly HttpClient _client;

        public AdminsControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        // =====================================
        // Helper CORRIGIDO: faz login e retorna o JWT (sem dynamic)
        // =====================================
        private async Task<string> ObterTokenAdminAsync()
        {
            var loginDto = new
            {
                email = "luciana@admin.com",
                senha = "985214"
            };

            var loginResponse = await _client.PostAsJsonAsync("login", loginDto);
            loginResponse.EnsureSuccessStatusCode();

            // Lê como JsonElement em vez de dynamic
            var jsonElement = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();

            // Extrai a propriedade "token"
            if (jsonElement.TryGetProperty("token", out JsonElement tokenElement))
            {
                string? token = tokenElement.GetString();
                return token ?? throw new InvalidOperationException("Token retornado veio nulo.");
            }

            throw new InvalidOperationException("Propriedade 'token' não encontrada no JSON de login.");
        }

        // ================================
        // 1) Testes de endpoint público
        // ================================

        [Fact(DisplayName = "Cadastro de Admin: sucesso")]
        public async Task CadastrarAdmin_DeveRetornar201()
        {
            var dto = new
            {
                nome = "Luciana Admin",
                email = $"lucia.{Guid.NewGuid()}@admin.com",
                senha = "985214",
                confirmacaoSenha = "985214"
            };

            var resp = await _client.PostAsJsonAsync("cadastrar", dto);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact(DisplayName = "Cadastro de Admin: email duplicado")]
        public async Task CadastrarAdmin_EmailDuplicado_DeveRetornar400()
        {
            var dto = new
            {
                nome = "Luciana Admin",
                email = "lucia@admin.com",
                senha = "985214",
                confirmacaoSenha = "985214"
            };

            var resp = await _client.PostAsJsonAsync("cadastrar", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "Login Admin: sucesso e token retornado")]
        public async Task LoginAdmin_DeveRetornar200EJwt()
        {
            var dto = new
            {
                email = "luciana@admin.com",
                senha = "985214"
            };

            var resp = await _client.PostAsJsonAsync("login", dto);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            // Lê a resposta como JsonElement
            var jsonElement = await resp.Content.ReadFromJsonAsync<JsonElement>();
            // Verifica se existe a propriedade "token"
            Assert.True(jsonElement.TryGetProperty("token", out JsonElement tok));
            Assert.False(string.IsNullOrEmpty(tok.GetString()));
        }

        [Fact(DisplayName = "Login Admin: senha errada")]
        public async Task LoginAdmin_SenhaIncorreta_DeveRetornar400()
        {
            var dto = new
            {
                email = "luciana@admin.com",
                senha = "senhaIncorreta"
            };

            var resp = await _client.PostAsJsonAsync("login", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ================================
        // 2) Testes de endpoints protegidos por JWT
        // ================================

        [Fact(DisplayName = "Get Admin por ID: sem JWT retorna 401")]
        public async Task GetAdminPorId_SemJwt_DeveRetornar401()
        {
            var resp = await _client.GetAsync("5282");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Get Admin por ID: sucesso com JWT")]
        public async Task GetAdminPorId_ComJwt_DeveRetornar200()
        {
            // 1) Obter token JWT via helper
            string token = await ObterTokenAdminAsync();

            // 2) Cria um HttpClient autenticado
            var clientAutenticado = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
            clientAutenticado.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3) Chama GET /{id} (use um ID válido, ex.: 5282)
            var resp = await clientAutenticado.GetAsync("5282");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact(DisplayName = "Editar Admin: sem JWT retorna 401")]
        public async Task EditarAdmin_SemJwt_DeveRetornar401()
        {
            var dto = new
            {
                nome = "Novo Nome",
                senhaAtual = "985214",
                novaSenha = "123456"
            };

            var resp = await _client.PatchAsJsonAsync("1/editar", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Editar Admin: com JWT, dados inválidos retorna 400")]
        public async Task EditarAdmin_ComJwt_DadosInvalidos_DeveRetornar400()
        {
            // 1) Obter token JWT via helper
            string token = await ObterTokenAdminAsync();

            // 2) Cria um HttpClient autenticado
            var clientAutenticado = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
            clientAutenticado.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3) Envia payload sem "senhaAtual" para forçar BadRequest
            var dto = new
            {
                nome = "Novo Nome Apenas",
                // senhaAtual ausente
                novaSenha = "1234568"
            };

            var resp = await clientAutenticado.PatchAsJsonAsync("1/editar", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "Deletar Admin: sem JWT retorna 401")]
        public async Task DeletarAdmin_SemJwt_DeveRetornar401()
        {
            var resp = await _client.DeleteAsync("1");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "Deletar Admin: com JWT e ID inválido retorna 404")]
        public async Task DeletarAdmin_ComJwt_IdInexistente_DeveRetornar404()
        {
            // 1) Obter token JWT via helper
            string token = await ObterTokenAdminAsync();

            // 2) Cria um HttpClient autenticado
            var clientAutenticado = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
            clientAutenticado.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3) Chama DELETE para um ID que não exista (por exemplo, 1894)
            var resp = await clientAutenticado.DeleteAsync("1894");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "Redefinir Senha de Admin: 200 ou 400 ou 404")]
        public async Task ForgotPassword_DeveRetornarStatusEsperado()
        {
            var dto = new
            {
                email = "luciana@admin.com",
                novaSenha = "NovaSenha123!",
                confirmacaoSenha = "NovaSenha123!"
            };

            var resp = await _client.PostAsJsonAsync("esqueceu-senha", dto);

            Assert.True(
                resp.StatusCode == HttpStatusCode.OK ||
                resp.StatusCode == HttpStatusCode.BadRequest ||
                resp.StatusCode == HttpStatusCode.NotFound
            );
        }
    }
}
