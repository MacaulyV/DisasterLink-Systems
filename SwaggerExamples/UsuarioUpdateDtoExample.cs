using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para atualização de dados do usuário.
    /// </summary>
    public class UsuarioUpdateDtoExample : IExamplesProvider<UsuarioUpdateDto>
    {
        public UsuarioUpdateDto GetExamples()
        {
            return new UsuarioUpdateDto
            {
                Nome = "Usuário Exemplo Atualizado",
                SenhaAtual = "654321",
                NovaSenha = "nova123",
                 
            };
        }
    }
} 