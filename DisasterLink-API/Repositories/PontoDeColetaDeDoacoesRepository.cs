using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterLink_API.Repositories
{
    public class PontoDeColetaDeDoacoesRepository : IPontoDeColetaDeDoacoesRepository
    {
        private readonly DisasterLinkDbContext _context;

        public PontoDeColetaDeDoacoesRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<PontoDeColetaDeDoacoes> AddAsync(PontoDeColetaDeDoacoes pontoDeColeta)
        {
            // Lógica para gerar ID de 4 dígitos aleatório e único
            var random = new Random();
            int newId;
            do
            {
                newId = random.Next(1000, 10000); // Gera um número entre 1000 e 9999
            }
            while (await _context.PontosDeColetaDeDoacoes.AnyAsync(p => p.Id == newId));
            
            pontoDeColeta.Id = newId;

            await _context.PontosDeColetaDeDoacoes.AddAsync(pontoDeColeta);
            await _context.SaveChangesAsync();
            return pontoDeColeta;
        }

        public async Task AddParticipacaoAsync(int pontoColetaId, ParticipacaoPontoColeta participacao)
        {
            var pontoColeta = await _context.PontosDeColetaDeDoacoes.Include(p => p.Participacoes).FirstOrDefaultAsync(p => p.Id == pontoColetaId);
            if (pontoColeta != null)
            {
                participacao.PontoColetaId = pontoColetaId;
                // O ID da participação é gerado pelo banco (Identity)
                _context.ParticipacoesPontoColeta.Add(participacao);
                await _context.SaveChangesAsync();
            }
            // Considerar lançar exceção se pontoColeta for null
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PontosDeColetaDeDoacoes.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PontoDeColetaDeDoacoes>> GetAllAsync(string? cidade, string? tipoDoacao)
        {
            var query = _context.PontosDeColetaDeDoacoes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(cidade))
            {
                query = query.Where(p => p.Cidade.Contains(cidade));
            }

            if (!string.IsNullOrWhiteSpace(tipoDoacao))
            {
                query = query.Where(p => p.Tipo.Contains(tipoDoacao));
            }

            return await query.Include(p => p.Participacoes).ToListAsync();
        }

        public async Task<PontoDeColetaDeDoacoes?> GetByIdAsync(int id)
        {
            return await _context.PontosDeColetaDeDoacoes.Include(p => p.Participacoes).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ParticipacaoPontoColeta>> GetParticipantesAsync(int pontoColetaId)
        {
            return await _context.ParticipacoesPontoColeta.Where(p => p.PontoColetaId == pontoColetaId).ToListAsync();
        }

        public async Task UpdateAsync(PontoDeColetaDeDoacoes pontoDeColeta)
        {
            _context.PontosDeColetaDeDoacoes.Update(pontoDeColeta);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pontoDeColeta = await _context.PontosDeColetaDeDoacoes.FindAsync(id);
            if (pontoDeColeta == null)
            {
                return false; // Não encontrado
            }

            _context.PontosDeColetaDeDoacoes.Remove(pontoDeColeta);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 