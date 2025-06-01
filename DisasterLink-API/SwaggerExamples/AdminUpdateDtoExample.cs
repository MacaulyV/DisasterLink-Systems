using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Admin;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para atualização de dados do administrador.
    /// </summary>
    public class AdminUpdateDtoExample : IExamplesProvider<AdminUpdateDto>
    {
        public AdminUpdateDto GetExamples()
        {
            return new AdminUpdateDto
            {
                Nome = "Luciana Admin Atualizada",
                SenhaAtual = "985214", // Senha atual atualizada
                NovaSenha = "123456", // Nova senha de exemplo
                ConfirmacaoNovaSenha = "123456" // Confirmação da nova senha
            };
        }
    }
} 