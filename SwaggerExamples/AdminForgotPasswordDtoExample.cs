using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para o endpoint "Esqueceu Senha" do Admin.
    /// </summary>
    public class AdminForgotPasswordDtoExample : IExamplesProvider<AdminForgotPasswordDto>
    {
        public AdminForgotPasswordDto GetExamples()
        {
            return new AdminForgotPasswordDto
            {
                Email = "luciana@admin.com",
                NovaSenha = "123456",
                ConfirmacaoNovaSenha = "123456"
            };
        }
    }
} 