using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterLink_API.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly DisasterLinkDbContext _context;

        public AdminRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<Admin> GetByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task<Admin> GetByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<List<Admin>> GetAllAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<Admin> AddAsync(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task UpdateAsync(Admin admin)
        {
            _context.Entry(admin).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email, int? currentAdminId = null)
        {
            if (currentAdminId.HasValue)
            {
                // Ao atualizar, permite que o email seja o mesmo do admin atual
                return await _context.Admins.AnyAsync(a => a.Email == email && a.Id != currentAdminId.Value);
            }
            return await _context.Admins.AnyAsync(a => a.Email == email);
        }

        public async Task<Admin?> GetLastAdminAsync()
        {
            return await _context.Admins.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
        }

        public async Task<bool> IdExistsAsync(int id)
        {
            return await _context.Admins.AnyAsync(a => a.Id == id);
        }
    }
} 