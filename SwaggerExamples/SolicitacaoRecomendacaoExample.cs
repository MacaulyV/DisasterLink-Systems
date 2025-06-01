using DisasterLink_API.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace DisasterLink_API.SwaggerExamples
{
    public class SolicitacaoRecomendacaoExample : IExamplesProvider<SolicitacaoRecomendacaoDTO>
    {
        public SolicitacaoRecomendacaoDTO GetExamples()
        {
            return new SolicitacaoRecomendacaoDTO
            {
                Necessidade = "kits de higiene",
                Cidade = "São Paulo"
            };
        }
    }

    public class SolicitacaoRecomendacaoAlimentosExample : IExamplesProvider<SolicitacaoRecomendacaoDTO>
    {
        public SolicitacaoRecomendacaoDTO GetExamples()
        {
            return new SolicitacaoRecomendacaoDTO
            {
                Necessidade = "alimentos não perecíveis",
                Cidade = "Porto Alegre"
            };
        }
    }

    public class SolicitacaoRecomendacaoFraldasExample : IExamplesProvider<SolicitacaoRecomendacaoDTO>
    {
        public SolicitacaoRecomendacaoDTO GetExamples()
        {
            return new SolicitacaoRecomendacaoDTO
            {
                Necessidade = "fraldas infantis",
                Cidade = "Campinas"
            };
        }
    }
} 