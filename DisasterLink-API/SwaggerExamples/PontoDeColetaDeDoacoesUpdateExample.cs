using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Update;

namespace DisasterLink_API.SwaggerExamples
{
    public class PontoDeColetaDeDoacoesUpdateExample : IExamplesProvider<PontoDeColetaDeDoacoesUpdateDto>
    {
        public PontoDeColetaDeDoacoesUpdateDto GetExamples()
        {
            return new PontoDeColetaDeDoacoesUpdateDto
            {
                Descricao = "Atualização: Agora também estamos Oferencendo cestas básicas e roupas.",
                Estoque = "cestas básicas, roupas"
            };
        }
    }
} 