using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Create;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para participação em Ponto de Coleta de Doações
    /// </summary>
    public class ParticipacaoPontoColetaCreateDtoExample : IExamplesProvider<ParticipacaoPontoColetaCreateDto>
    {
        public ParticipacaoPontoColetaCreateDto GetExamples()
        {
            return new ParticipacaoPontoColetaCreateDto
            {
                FormaDeAjuda = "Doação de roupas",
                Mensagem = "Posso doar casacos e calças em ótimo estado. Entregarei na próxima semana.",
                Contato = "maria@exemplo.com",
                Telefone = "(21) 99876-5432"
            };
        }
    }
} 