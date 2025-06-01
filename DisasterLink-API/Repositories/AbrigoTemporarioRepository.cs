using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using System;

namespace DisasterLink_API.Repositories
{
    /// <summary>
    /// Implementação de IAbrigoTemporarioRepository usando EF Core
    /// </summary>
    public class AbrigoTemporarioRepository : IAbrigoTemporarioRepository
    {
        private readonly DisasterLinkDbContext _context;

        public AbrigoTemporarioRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<AbrigoTemporario> AddAsync(AbrigoTemporario abrigoTemporario)
        {
            // Lógica para gerar ID de 4 dígitos aleatório e único
            var random = new Random();
            int newId;
            bool idExists;
            do
            {
                newId = random.Next(1000, 10000); // Gera um número entre 1000 e 9999
                // Verifica se o ID já existe no DbSet atual (antes de SaveChanges)
                // ou no banco de dados se a consulta for feita diretamente ao banco.
                idExists = await _context.AbrigosTemporarios.AnyAsync(a => a.Id == newId);
                if (!idExists)
                {
                    // Adicionalmente, verifica se o ID já foi adicionado na sessão atual do DbContext mas ainda não salvo
                    // Isso é importante se múltiplos itens forem adicionados em uma única transação SaveChanges.
                    idExists = _context.ChangeTracker.Entries<AbrigoTemporario>()
                                        .Any(e => e.State == EntityState.Added && e.Entity.Id == newId);
                }
            }
            while (idExists);
            
            abrigoTemporario.Id = newId;

            _context.AbrigosTemporarios.Add(abrigoTemporario); 
            await _context.SaveChangesAsync();
            return abrigoTemporario;
        }

        public async Task DeleteAsync(AbrigoTemporario abrigoTemporario)
        {
            _context.AbrigosTemporarios.Remove(abrigoTemporario);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AbrigoTemporario>> GetAllAsync()
        {
            return await _context.AbrigosTemporarios.AsNoTracking().ToListAsync();
        }

        public async Task<AbrigoTemporario?> GetByIdAsync(int id)
        {
            return await _context.AbrigosTemporarios.FindAsync(id);
        }

        public async Task<IEnumerable<AbrigoTemporario>> GetByCityAsync(string city)
        {
            return await _context.AbrigosTemporarios
                .Where(a => EF.Functions.Like(a.CidadeMunicipio, city)) // Usando EF.Functions.Like para case-insensitive
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(AbrigoTemporario abrigoTemporario)
        {
            _context.AbrigosTemporarios.Update(abrigoTemporario);
            await _context.SaveChangesAsync();
        }
    }
} 