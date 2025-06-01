using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace DisasterLink_API.Repositories
{
    public class VisualizacaoAlertaRepository : IVisualizacaoAlertaRepository
    {
        private readonly DisasterLinkDbContext _context;

        public VisualizacaoAlertaRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteVisualizacaoAsync(int alertaId, int usuarioId)
        {
            return await _context.VisualizacoesAlerta
                .AsNoTracking()
                .AnyAsync(va => va.AlertaId == alertaId && va.UsuarioId == usuarioId);
        }

        public async Task AdicionarVisualizacaoAsync(VisualizacaoAlerta visualizacao)
        {
            // A DataVisualizacao deve ser definida no serviço com o fuso correto (Brasília)
            await _context.VisualizacoesAlerta.AddAsync(visualizacao);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<VisualizacaoAlerta>> GetVisualizacoesPorUsuarioAsync(int usuarioId)
        {
            return await _context.VisualizacoesAlerta
                .AsNoTracking()
                .Where(va => va.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<VisualizacaoAlerta>> GetVisualizacoesPorAlertaAsync(int alertaId)
        {
            return await _context.VisualizacoesAlerta
                .Where(va => va.AlertaId == alertaId) // Não usar AsNoTracking se formos deletar
                .ToListAsync();
        }

        public async Task RemoverRangeVisualizacoes(IEnumerable<VisualizacaoAlerta> visualizacoes)
        {
            _context.VisualizacoesAlerta.RemoveRange(visualizacoes);
            await _context.SaveChangesAsync();
        }
    }
} 