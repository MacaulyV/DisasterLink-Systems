using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System; // Necessário para Random
using System.Linq;

namespace DisasterLink_API.Repositories
{
    public class ParticipacaoPontoColetaRepository : IParticipacaoPontoColetaRepository
    {
        private readonly DisasterLinkDbContext _context;

        public ParticipacaoPontoColetaRepository(DisasterLinkDbContext context)
        {
            _context = context;
        }

        public async Task<ParticipacaoPontoColeta> AddAsync(ParticipacaoPontoColeta participacao)
        {
            // Lógica para gerar ID de 4 dígitos aleatório e único para a participação
            var random = new Random();
            int newId;
            do
            {
                newId = random.Next(1000, 10000); // Gera um número entre 1000 e 9999
            }
            while (await _context.ParticipacoesPontoColeta.AnyAsync(p => p.Id == newId));
            
            participacao.Id = newId;

            // A PontoColetaId, IdUsuario e DataHora devem ser definidos no serviço antes de chamar este método.
            await _context.ParticipacoesPontoColeta.AddAsync(participacao);
            await _context.SaveChangesAsync();
            return participacao;
        }

        public async Task<int> DeleteByPontoColetaIdAsync(int pontoColetaId)
        {
            // EF Core 7+ suporta ExecuteDeleteAsync para exclusão em massa eficiente
            var rowCount = await _context.ParticipacoesPontoColeta
                .Where(p => p.PontoColetaId == pontoColetaId)
                .ExecuteDeleteAsync();
            
            // Não é necessário SaveChangesAsync() após ExecuteDeleteAsync()
            return rowCount;
        }
    }
} 