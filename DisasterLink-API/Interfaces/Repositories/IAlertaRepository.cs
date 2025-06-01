using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para acesso a dados de Alertas
    /// </summary>
    public interface IAlertaRepository
    {
        Task<Alerta> AddAsync(Alerta alerta);
        Task<IEnumerable<Alerta>> GetAllAsync();
        Task<IEnumerable<Alerta>> GetByCityAsync(string city);
        Task<IEnumerable<Alerta>> GetByTipoAsync(string tipo);
        Task<Alerta?> GetByIdAsync(int id);
        Task<Alerta?> GetByOrigemAsync(int origemId, TipoOrigemAlerta tipoOrigem);
        Task<bool> DeleteAsync(int id);
        Task<int> DeleteAllAsync();
    }
} 