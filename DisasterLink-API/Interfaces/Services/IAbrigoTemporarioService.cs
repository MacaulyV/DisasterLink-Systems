using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.DTOs;

namespace DisasterLink_API.Interfaces.Services
{
    /// <summary>
    /// Contrato de lógica de negócio para Abrigos Temporários
    /// </summary>
    public interface IAbrigoTemporarioService // Renomeado de IOcorrenciaService
    {
        // O parâmetro usuarioId foi mantido, mas sua necessidade/uso deve ser revisado
        // já que a entidade AbrigoTemporario não possui mais um vínculo direto com Usuario.
        // Pode ser usado para log de quem cadastrou, por exemplo.
        Task<AbrigoTemporarioDto> CreateAsync(AbrigoTemporarioCreateDto dto, int usuarioId);
        Task<IEnumerable<AbrigoTemporarioDto>> GetAllAsync();
        Task<AbrigoTemporarioDto?> GetByIdAsync(int id);
        Task<IEnumerable<AbrigoTemporarioDto>> GetByCityAsync(string city);
        // GetRecentAsync removido
        // GetByUserIdAsync removido
        Task UpdateAsync(int id, AbrigoTemporarioCreateDto dto);
        Task DeleteAsync(int id);
    }
} 