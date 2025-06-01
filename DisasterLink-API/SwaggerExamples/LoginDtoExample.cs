using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para login de usuário
    /// </summary>
    public class LoginDtoExample : IExamplesProvider<LoginDto>
    {
        public LoginDto GetExamples()
        {
            return new LoginDto
            {
                Email = "carlos@exemplo.com",
                Senha = "654321"
            };
        }
    }
} 