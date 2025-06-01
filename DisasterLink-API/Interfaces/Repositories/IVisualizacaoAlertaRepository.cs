using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Interfaces.Repositories
{
    public interface IVisualizacaoAlertaRepository
    {
        Task<bool> ExisteVisualizacaoAsync(int alertaId, int usuarioId);
        Task AdicionarVisualizacaoAsync(VisualizacaoAlerta visualizacao);
        Task<IEnumerable<VisualizacaoAlerta>> GetVisualizacoesPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<VisualizacaoAlerta>> GetVisualizacoesPorAlertaAsync(int alertaId);
        Task RemoverRangeVisualizacoes(IEnumerable<VisualizacaoAlerta> visualizacoes);
    }
} 