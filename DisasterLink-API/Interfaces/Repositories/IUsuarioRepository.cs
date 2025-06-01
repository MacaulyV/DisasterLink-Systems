using System.Threading.Tasks;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para acesso a dados de usuários
    /// </summary>
    public interface IUsuarioRepository
    {
        Task<Usuario> AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByGoogleUserIdAsync(string googleUserId);
        Task UpdateAsync(Usuario usuario);
        Task<List<Usuario>> GetAllAsync();
        Task<List<Usuario>> GetByLocalidadeAsync(string? cidadeMunicipio, string? bairro);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
} 