using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Admin;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para criação de administrador
    /// </summary>
    public class AdminCreateDtoExample : IExamplesProvider<AdminCreateDto>
    {
        public AdminCreateDto GetExamples()
        {
            return new AdminCreateDto
            {
                Nome = "Luciana Admin",
                Email = "luciana@admin.com",
                Senha = "985214",
                ConfirmacaoSenha = "985214"
            };
        }
    }
} 