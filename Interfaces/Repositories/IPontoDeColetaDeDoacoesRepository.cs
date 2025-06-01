using DisasterLink_API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterLink_API.Interfaces.Repositories
{
    public interface IPontoDeColetaDeDoacoesRepository
    {
        Task<IEnumerable<PontoDeColetaDeDoacoes>> GetAllAsync(string? cidade, string? tipoDoacao);
        Task<PontoDeColetaDeDoacoes?> GetByIdAsync(int id);
        Task<PontoDeColetaDeDoacoes> AddAsync(PontoDeColetaDeDoacoes pontoDeColeta);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ParticipacaoPontoColeta>> GetParticipantesAsync(int pontoColetaId);
        Task AddParticipacaoAsync(int pontoColetaId, ParticipacaoPontoColeta participacao);
        Task UpdateAsync(PontoDeColetaDeDoacoes pontoDeColeta);
        Task<bool> DeleteAsync(int id);
    }
} 