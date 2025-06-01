using DisasterLink_API.Entities;
using System.Threading.Tasks;

namespace DisasterLink_API.Interfaces.Repositories
{
    public interface IParticipacaoPontoColetaRepository
    {
        Task<ParticipacaoPontoColeta> AddAsync(ParticipacaoPontoColeta participacao);
        Task<int> DeleteByPontoColetaIdAsync(int pontoColetaId);
        // Adicionar outros métodos se necessário no futuro, como GetById, GetAllByPontoColetaId, etc.
    }
} 