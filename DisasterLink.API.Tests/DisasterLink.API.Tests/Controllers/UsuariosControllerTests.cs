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
    public class UsuariosControllerTests
    {
        // Base URL para os endpoints de usuário
        private readonly HttpClient _client;
        // Base URL para login de admin (para testes que requerem JWT de admin)
        private readonly HttpClient _adminClient;

        public UsuariosControllerTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/usuarios/")
            };
            _adminClient = new HttpClient
            {
                BaseAddress = new Uri("https://disasterlink-api.fly.dev/api/admin/")
            };
        }

        // ===================================================================
        // Helper para obter token de Admin (reuse de teste de AdminsController)
        // ===================================================================
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

        // ========================================
        // 1) Testes para POST /api/usuarios/cadastrar
        // ========================================

        [Fact(DisplayName = "Cadastro de Usuário: sucesso")]
        public async Task CreateUsuario_DeveRetornar201()
        {
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var dto = new
            {
                nome = $"Teste Usuário {unique}",
                email = $"teste.user.{unique}@email.com",
                senha = "Senha123",
                telefone = "11987654321"
                // campos opcionais omitidos
            };

            var resp = await _client.PostAsJsonAsync("cadastrar", dto);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact(DisplayName = "Cadastro de Usuário: dados obrigatórios ausentes retorna 400")]
        public async Task CreateUsuario_DadosInvalidos_DeveRetornar400()
        {
            // Falta campo obrigatório "telefone"
            var dto = new
            {
                nome = "Usuário Inválido",
                email = "invalido.email",
                senha = "123" // senha muito curta
            };

            var resp = await _client.PostAsJsonAsync("cadastrar", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "Cadastro de Usuário: email duplicado retorna 400")]
        public async Task CreateUsuario_EmailDuplicado_DeveRetornar400()
        {
            // Supondo que "luciana@admin.com" esteja cadastrado, mas é admin, 
            // vamos cadastrar um usuário fixo antes, se possível.
            // Como não temos controle se já existe usuário com esse email,
            // usamos um email fixo que sabemos que já falha.
            var dto = new
            {
                nome = "Teste Duplicado",
                email = "luciana@admin.com", // email já existe (admin), gerará 400
                senha = "Senha123",
                telefone = "11987654321"
            };

            var resp = await _client.PostAsJsonAsync("cadastrar", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ========================================
        // 2) Testes para POST /api/usuarios/login
        // ========================================

        [Fact(DisplayName = "Login Usuário: sucesso retorna 200")]
        public async Task LoginUsuario_DeveRetornar200()
        {
            // Para garantir um login válido, primeiro criamos um usuário de teste
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var emailTeste = $"login.user.{unique}@email.com";
            var criarDto = new
            {
                nome = $"Usuário Login {unique}",
                email = emailTeste,
                senha = "Senha123",
                telefone = "11987654321"
            };
            var criarResp = await _client.PostAsJsonAsync("cadastrar", criarDto);
            criarResp.EnsureSuccessStatusCode();

            var loginDto = new
            {
                email = emailTeste,
                senha = "Senha123"
            };
            var resp = await _client.PostAsJsonAsync("login", loginDto);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            // Verifica se o JSON de retorno contém "sucesso" e "usuario" (ou ao menos "sucesso": true)
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("sucesso", out JsonElement sucessoElem));
            Assert.True(sucessoElem.GetBoolean());
        }

        [Fact(DisplayName = "Login Usuário: credenciais inválidas retorna 401")]
        public async Task LoginUsuario_CredenciaisInvalidas_DeveRetornar401()
        {
            var loginDto = new
            {
                email = "nao.existe@teste.com",
                senha = "Senha123"
            };
            var resp = await _client.PostAsJsonAsync("login", loginDto);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        // ========================================
        // 3) Testes para GET /api/usuarios/{id}
        // ========================================

        [Fact(DisplayName = "GetById Usuário: público, id inexistente retorna 404")]
        public async Task GetUsuarioById_Inexistente_DeveRetornar404()
        {
            var resp = await _client.GetAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "GetById Usuário: público, id existente retorna 200")]
        public async Task GetUsuarioById_Existente_DeveRetornar200()
        {
            // Cria um usuário para garantir que existe
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Usuário Pega1 {unique}",
                email = $"pega1.user.{unique}@email.com",
                senha = "Senha123",
                telefone = "11987654321"
            };
            var criarResp = await _client.PostAsJsonAsync("cadastrar", criarDto);
            criarResp.EnsureSuccessStatusCode();

            // Extrai o ID criado a partir da Location header ou do corpo
            int idCriado;
            var content = await criarResp.Content.ReadFromJsonAsync<JsonElement>();
            if (content.TryGetProperty("id", out JsonElement idElem))
            {
                idCriado = idElem.GetInt32();
            }
            else
            {
                // Tenta ler Location header no formato ".../api/usuarios/{id}"
                var location = criarResp.Headers.Location?.AbsolutePath; // "/api/usuarios/123"
                var partes = location?.Split('/');
                idCriado = int.Parse(partes![^1]);
            }

            var resp = await _client.GetAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        // ========================================
        // 4) Testes para GET /api/usuarios (Admin only)
        // ========================================

        [Fact(DisplayName = "GetAll Usuários: sem token retorna 401")]
        public async Task GetAllUsuarios_SemJwt_DeveRetornar401()
        {
            var resp = await _client.GetAsync("");
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact(DisplayName = "GetAll Usuários: com JWT de admin retorna 200")]
        public async Task GetAllUsuarios_ComJwt_DeveRetornar200()
        {
            string token = await ObterTokenAdminAsync();
            var clientAut = new HttpClient { BaseAddress = _client.BaseAddress };
            clientAut.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var resp = await clientAut.GetAsync("");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            // Verifica se retorna uma lista JSON
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.ValueKind == JsonValueKind.Array);
        }

        // ========================================
        // 5) Testes para POST /api/usuarios/esqueceu-senha
        // ========================================

        [Fact(DisplayName = "ForgotPassword: email inexistente retorna 400 ou 404")]
        public async Task ForgotPassword_EmailInexistente_DeveRetornar400Ou404()
        {
            var dto = new
            {
                email = "carlos@exemplo.com",          // e-mail que não existe
                novaSenha = "SenhaNova1!",
                confirmacaoSenha = "SenhaNova1!"
            };
            var resp = await _client.PostAsJsonAsync("esqueceu-senha", dto);

            // Agora aceitamos tanto 400 (BadRequest) quanto 404 (NotFound)
            Assert.True(
                resp.StatusCode == HttpStatusCode.NotFound ||
                resp.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Fact(DisplayName = "ForgotPassword: senhas não conferem retorna 400")]
        public async Task ForgotPassword_SenhasDiferentes_DeveRetornar400()
        {
            // Primeiro cria usuário
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Usuário Esqueceu1 {unique}",
                email = $"luciana.{unique}@admin.com",
                senha = "Senha123",
                telefone = "11987654321"
            };
            var criarResp = await _client.PostAsJsonAsync("cadastrar", criarDto);
            criarResp.EnsureSuccessStatusCode();

            var dto = new
            {
                email = $"esqueceu1.user.{unique}@email.com",
                novaSenha = "Nova1!",
                confirmacaoSenha = "NaoBate!"
            };
            var resp = await _client.PostAsJsonAsync("esqueceu-senha", dto);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // ========================================
        // 6) Testes para GET /api/usuarios/localidade
        // ========================================

        [Fact(DisplayName = "GetByLocalidade: sem parâmetros retorna 400")]
        public async Task GetByLocalidade_SemParametros_DeveRetornar400()
        {
            var resp = await _client.GetAsync("localidade");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact(DisplayName = "GetByLocalidade: sem resultados retorna 404")]
        public async Task GetByLocalidade_NenhumResultado_DeveRetornar404()
        {
            var resp = await _client.GetAsync("localidade?cidadeMunicipio=CidadeQueNaoExiste&bairro=BairroNaoExiste");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        // ========================================
        // 7) Teste de DELETE /api/usuarios/{id}
        // ========================================

        [Fact(DisplayName = "Delete Usuário: id inexistente retorna 404")]
        public async Task DeleteUsuario_IdInexistente_DeveRetornar404()
        {
            var resp = await _client.DeleteAsync("999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "Delete Usuário: cria e deleta retorna 204")]
        public async Task DeleteUsuario_CriaEDeleta_DeveRetornar204()
        {
            // Cria usuário
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Usuário Exclui {unique}",
                email = $"exclui.user.{unique}@email.com",
                senha = "Senha123",
                telefone = "11987654321"
            };
            var criarResp = await _client.PostAsJsonAsync("cadastrar", criarDto);
            criarResp.EnsureSuccessStatusCode();

            // Extrai ID criado
            int idCriado;
            var content = await criarResp.Content.ReadFromJsonAsync<JsonElement>();
            if (content.TryGetProperty("id", out JsonElement idElem))
            {
                idCriado = idElem.GetInt32();
            }
            else
            {
                // Usa Location header
                var location = criarResp.Headers.Location?.AbsolutePath;
                var partes = location?.Split('/');
                idCriado = int.Parse(partes![^1]);
            }

            // Deleta
            var respDelete = await _client.DeleteAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.NoContent, respDelete.StatusCode);

            // Confirma que foi removido
            var respGet = await _client.GetAsync(idCriado.ToString());
            Assert.Equal(HttpStatusCode.NotFound, respGet.StatusCode);
        }

        // ========================================
        // 8) Teste PATCH /api/usuarios/{id}
        // ========================================

        [Fact(DisplayName = "Update Usuário: id inexistente retorna 404")]
        public async Task UpdateUsuario_IdInexistente_DeveRetornar404()
        {
            var dto = new
            {
                nome = "Nome Novo",
                telefone = "11900000000"
            };
            var resp = await _client.PatchAsJsonAsync("999999", dto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact(DisplayName = "Update Usuário: cria e atualiza retorna 204")]
        public async Task UpdateUsuario_CriaEAtualiza_DeveRetornar204()
        {
            // Cria usuário
            var unique = Guid.NewGuid().ToString().Substring(0, 8);
            var criarDto = new
            {
                nome = $"Usuário Atualiza {unique}",
                email = $"atualiza.user.{unique}@email.com",
                senha = "Senha123",
                telefone = "11987654321"
            };
            var criarResp = await _client.PostAsJsonAsync("cadastrar", criarDto);
            criarResp.EnsureSuccessStatusCode();

            // Extrai ID criado
            int idCriado;
            var content = await criarResp.Content.ReadFromJsonAsync<JsonElement>();
            if (content.TryGetProperty("id", out JsonElement idElem))
            {
                idCriado = idElem.GetInt32();
            }
            else
            {
                // Usa Location header
                var location = criarResp.Headers.Location?.AbsolutePath;
                var partes = location?.Split('/');
                idCriado = int.Parse(partes![^1]);
            }

            // Atualiza
            var dtoUpdate = new
            {
                nome = $"Nome Atualizado {unique}",
                telefone = "11911111111"
            };
            var respUpdate = await _client.PatchAsJsonAsync(idCriado.ToString(), dtoUpdate);
            Assert.Equal(HttpStatusCode.NoContent, respUpdate.StatusCode);

            // Confirma alteração via GET
            var respGet = await _client.GetAsync(idCriado.ToString());
            respGet.EnsureSuccessStatusCode();
            var usuarioJson = await respGet.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(usuarioJson.TryGetProperty("nome", out JsonElement nomeElem));
            Assert.Equal($"Nome Atualizado {unique}", nomeElem.GetString());
        }
    }
}
