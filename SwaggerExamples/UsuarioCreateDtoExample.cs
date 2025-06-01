using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para criação de usuário
    /// </summary>
    public class UsuarioCreateDtoExample : IExamplesProvider<UsuarioCreateDto>
    {
        public UsuarioCreateDto GetExamples()
        {
            // Exemplo com campos de endereço opcionais omitidos (ou nulos)
            return new UsuarioCreateDto
            {
                Nome = "Carlos Silva",
                Email = "carlos@exemplo.com",
                Senha = "654321",
                Pais = null, // Opcional
                Estado = null, // Opcional
                CidadeMunicipio = null, // Opcional
                Bairro = null // Opcional
            };

            /*
            // Exemplo com todos os campos de endereço preenchidos:
            return new UsuarioCreateDto
            {
                Nome = "Juliana Santos",
                Email = "juliana.completo@exemplo.com",
                Senha = "senha123",
                Pais = "Brasil",
                Estado = "Rio de Janeiro",
                CidadeMunicipio = "Rio de Janeiro",
                Bairro = "Copacabana"
            };
            */
        }
    }
} 