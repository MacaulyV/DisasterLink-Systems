using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;

namespace DisasterLink_API.Repositories
{
    /// <summary>
    /// Implementação de IUsuarioRepository usando EF Core
    /// </summary>
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DisasterLinkDbContext _context;
        public UsuarioRepository(DisasterLinkDbContext context) => _context = context;

        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            // Gerar ID aleatório de 4 dígitos
            usuario.Id = new Random().Next(1000, 10000);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Usuario?> GetByGoogleUserIdAsync(string googleUserId)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.GoogleUserId == googleUserId);
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.AsNoTracking().ToListAsync();
        }

        public async Task<List<Usuario>> GetByLocalidadeAsync(string? cidadeMunicipio, string? bairro)
        {
            var query = _context.Usuarios.AsQueryable();

            bool hasCidade = !string.IsNullOrWhiteSpace(cidadeMunicipio);
            bool hasBairro = !string.IsNullOrWhiteSpace(bairro);

            if (hasCidade && hasBairro)
            {
                // Filtra por cidade/município OU bairro (OR)
                query = query.Where(u => 
                    EF.Functions.Like(u.Municipio!, $"%{cidadeMunicipio}%") || 
                    EF.Functions.Like(u.Bairro!, $"%{bairro}%")
                );
            }
            else if (hasCidade)
            {
                // Filtra apenas por cidade/município
                query = query.Where(u => EF.Functions.Like(u.Municipio!, $"%{cidadeMunicipio}%"));
            }
            else if (hasBairro)
            {
                // Filtra apenas por bairro
                query = query.Where(u => EF.Functions.Like(u.Bairro!, $"%{bairro}%"));
            }
            // Se nenhum dos dois for fornecido, o controller já trata e retorna BadRequest.
            // Se quisesse retornar todos aqui, não adicionaria nenhum Where.

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            // Considerar lançar uma exceção se o usuário não for encontrado,
            // dependendo da política de tratamento de erros desejada.
            // Por exemplo: throw new KeyNotFoundException("Usuário não encontrado.");
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Usuarios.AnyAsync(u => u.Id == id);
        }
    }
} 