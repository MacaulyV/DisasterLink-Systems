using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.DTOs;
using DisasterLink_API.DTOs.Response;

namespace DisasterLink_API.Interfaces.Services
{
    /// <summary>
    /// Contrato de lógica de negócio para Alertas
    /// </summary>
    public interface IAlertaService
    {
        Task<AlertaDto> CreateAsync(AlertaCreateDto dto);
        Task<AlertaDto> CriarAlertaAutomaticoDeAbrigoAsync(AbrigoTemporarioDto abrigoDto);
        Task<AlertaDto> CriarAlertaAutomaticoDePontoColetaAsync(PontoDeColetaDeDoacoesDto pontoColetaDto);
        Task<IEnumerable<AlertaDto>> GetAllAsync();
        Task<IEnumerable<AlertaDto>> GetByCityAsync(string city, int idUsuario);
        Task<IEnumerable<AlertaDto>> GetByTipoAsync(string tipo);
        Task<AlertaDto?> GetAlertaByIdAsync(int id);
        Task<bool> DescartarAlertaParaUsuarioAsync(int alertaId, int idUsuario);
        Task<bool> DeleteAlertaAsync(int id);
        Task<bool> DeleteAlertaPorOrigemAsync(int origemId, Entities.TipoOrigemAlerta tipoOrigem);
        Task<int> DeleteTodosAlertasAsync();
    }
} 