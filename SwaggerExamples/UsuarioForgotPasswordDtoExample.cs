using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para o endpoint "Esqueceu Senha" do Usuário.
    /// </summary>
    public class UsuarioForgotPasswordDtoExample : IExamplesProvider<UsuarioForgotPasswordDto>
    {
        public UsuarioForgotPasswordDto GetExamples()
        {
            return new UsuarioForgotPasswordDto
            {
                Email = "carlos@exemplo.com", // Email de exemplo para usuário
                NovaSenha = "nova123",
                ConfirmacaoNovaSenha = "nova123"
            };
        }
    }
} 