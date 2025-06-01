using DisasterLink_API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisasterLink_API.Interfaces.Repositories
{
    public interface IAdminRepository
    {
        Task<Admin> GetByIdAsync(int id);
        Task<Admin> GetByEmailAsync(string email);
        Task<List<Admin>> GetAllAsync();
        Task<Admin> AddAsync(Admin admin);
        Task UpdateAsync(Admin admin);
        Task DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? currentAdminId = null);
        Task<Admin?> GetLastAdminAsync();
        Task<bool> IdExistsAsync(int id);
    }
} 