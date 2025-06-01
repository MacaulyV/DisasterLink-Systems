using DisasterLink_API.DTOs.Create;
using DisasterLink_API.DTOs.Response;
using DisasterLink_API.DTOs.Update;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterLink_API.Interfaces.Services
{
    public interface IPontoDeColetaDeDoacoesService
    {
        Task<IEnumerable<PontoDeColetaDeDoacoesDto>> GetAllPontosDeColetaAsync(string? cidade, string? tipoDoacao);
        Task<PontoDeColetaDeDoacoesDto?> GetPontoDeColetaByIdAsync(int id);
        Task<PontoDeColetaDeDoacoesDto> CreatePontoDeColetaAsync(PontoDeColetaDeDoacoesCreateDto pontoDeColetaDto);
        Task<ParticipacaoPontoColetaDto?> AddParticipacaoAsync(int pontoColetaId, int idUsuario, ParticipacaoPontoColetaCreateDto participacaoDto);
        Task<IEnumerable<ParticipacaoPontoColetaDto>> GetParticipantesAsync(int pontoColetaId);
        Task<PontoDeColetaDeDoacoesDto?> UpdatePontoDeColetaAsync(int id, PontoDeColetaDeDoacoesUpdateDto pontoDeColetaDto);
        Task<bool> DeletePontoDeColetaAsync(int id);
    }
} 