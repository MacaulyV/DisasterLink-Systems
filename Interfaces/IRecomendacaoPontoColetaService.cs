using DisasterLink_API.MLModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterLink_API.Interfaces
{
    public interface IRecomendacaoPontoColetaService
    {
        Task InicializarModeloAsync();
        Task<List<PontoColetaResultado>> ObterRecomendacoesAsync(string necessidade, string? cidade = null);
        Task AtualizarModeloAsync();
    }
} 