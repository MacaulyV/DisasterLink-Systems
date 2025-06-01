using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;

namespace DisasterLink_API.Repositories
{
    /// <summary>
    /// Implementação de IAlertaRepository usando EF Core
    /// </summary>
    public class AlertaRepository : IAlertaRepository
    {
        private readonly DisasterLinkDbContext _context;

        public AlertaRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<Alerta> AddAsync(Alerta alerta)
        {
            // Gerar ID aleatório de 4 dígitos
            alerta.Id = new Random().Next(1000, 10000);
            
            // Definir DataHora para o horário de Brasília em tempo real
            try
            {
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                alerta.DataHora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brasiliaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback para UTC caso o fuso horário de Brasília não seja encontrado
                // Considerar logar um aviso aqui ou tratar de forma mais robusta
                alerta.DataHora = DateTime.UtcNow;
            }
            catch (ArgumentNullException) // Adicionado para o caso de Id ser nulo, embora improvável aqui
            {
                 alerta.DataHora = DateTime.UtcNow; // Fallback
            }

            _context.Alertas.Add(alerta);
            await _context.SaveChangesAsync();
            return alerta;
        }

        public async Task<IEnumerable<Alerta>> GetAllAsync()
        {
            return await _context.Alertas
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Alerta>> GetByCityAsync(string city)
        {
            return await _context.Alertas
                .Where(a => a.Cidade.ToLower() == city.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Alerta>> GetByTipoAsync(string tipo)
        {
            return await _context.Alertas
                .Where(a => a.Tipo.ToLower() == tipo.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Alerta?> GetByIdAsync(int id)
        {
            return await _context.Alertas.FindAsync(id);
        }

        public async Task<Alerta?> GetByOrigemAsync(int origemId, TipoOrigemAlerta tipoOrigem)
        {
            return await _context.Alertas
                .FirstOrDefaultAsync(a => a.OrigemId == origemId && a.TipoOrigem == tipoOrigem);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var alerta = await _context.Alertas.FindAsync(id);
            if (alerta == null)
            {
                return false;
            }

            _context.Alertas.Remove(alerta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllAsync()
        {
            // EF Core não tem um método direto para deletar todos sem carregar, 
            // então usamos ExecuteDeleteAsync para eficiência.
            var rowCount = await _context.Alertas.ExecuteDeleteAsync();
            // Não é necessário SaveChangesAsync() após ExecuteDeleteAsync()
            return rowCount;
        }
    }
} 