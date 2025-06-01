using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para acesso a dados de Abrigos Temporários
    /// </summary>
    public interface IAbrigoTemporarioRepository
    {
        Task<AbrigoTemporario> AddAsync(AbrigoTemporario abrigoTemporario);
        Task<AbrigoTemporario?> GetByIdAsync(int id);
        Task<IEnumerable<AbrigoTemporario>> GetAllAsync();
        Task<IEnumerable<AbrigoTemporario>> GetByCityAsync(string city);
        Task UpdateAsync(AbrigoTemporario abrigoTemporario);
        Task DeleteAsync(AbrigoTemporario abrigoTemporario);
    }
} 