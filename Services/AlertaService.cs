using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DisasterLink_API.DTOs;
using DisasterLink_API.DTOs.Create;
using DisasterLink_API.DTOs.Response;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Interfaces.Services;
using System; // Adicionado para DateTime e TimeZoneInfo

namespace DisasterLink_API.Services
{
    /// <summary>
    /// Implementa lógica de negócio para Alertas
    /// </summary>
    public class AlertaService : IAlertaService
    {
        private readonly IAlertaRepository _alertaRepo;
        private readonly IVisualizacaoAlertaRepository _visualizacaoRepo;
        private readonly IUsuarioRepository _usuarioRepo; // Para verificar se o usuário existe
        private readonly IMapper _mapper;

        public AlertaService(IAlertaRepository alertaRepo, 
                             IVisualizacaoAlertaRepository visualizacaoRepo, 
                             IUsuarioRepository usuarioRepo, 
                             IMapper mapper)
        {
            _alertaRepo = alertaRepo;
            _visualizacaoRepo = visualizacaoRepo;
            _usuarioRepo = usuarioRepo;
            _mapper = mapper;
        }

        public async Task<AlertaDto> CreateAsync(AlertaCreateDto dto)
        {
            var entity = _mapper.Map<Alerta>(dto);
            var created = await _alertaRepo.AddAsync(entity);
            return _mapper.Map<AlertaDto>(created);
        }

        /// <summary>
        /// Cria automaticamente um alerta quando um abrigo temporário é criado
        /// </summary>
        public async Task<AlertaDto> CriarAlertaAutomaticoDeAbrigoAsync(AbrigoTemporarioDto abrigoDto)
        {
            var alertaDto = new AlertaCreateDto
            {
                Tipo = "Abrigo Temporário",
                Titulo = abrigoDto.Nome,
                Descricao = abrigoDto.Descricao,
                Cidade = abrigoDto.CidadeMunicipio,
                Bairro = abrigoDto.Bairro,
                Logradouro = abrigoDto.Logradouro,
                OrigemId = abrigoDto.Id,
                TipoOrigem = TipoOrigemAlerta.AbrigoTemporario
            };

            return await CreateAsync(alertaDto);
        }

        /// <summary>
        /// Cria automaticamente um alerta quando um ponto de coleta é criado
        /// </summary>
        public async Task<AlertaDto> CriarAlertaAutomaticoDePontoColetaAsync(PontoDeColetaDeDoacoesDto pontoColetaDto)
        {
            var alertaDto = new AlertaCreateDto
            {
                Tipo = "Ponto de Coleta",
                Titulo = pontoColetaDto.Tipo,
                Descricao = pontoColetaDto.Descricao,
                Cidade = pontoColetaDto.Cidade,
                Bairro = pontoColetaDto.Bairro,
                Logradouro = pontoColetaDto.Logradouro,
                OrigemId = pontoColetaDto.Id,
                TipoOrigem = TipoOrigemAlerta.PontoColeta
            };

            return await CreateAsync(alertaDto);
        }

        public async Task<IEnumerable<AlertaDto>> GetAllAsync()
        {
            var list = await _alertaRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<AlertaDto>>(list);
        }

        public async Task<IEnumerable<AlertaDto>> GetByCityAsync(string city, int idUsuario)
        {
            if (!await _usuarioRepo.ExistsAsync(idUsuario))
            {
                // Ou lançar uma exceção específica para usuário não encontrado
                throw new KeyNotFoundException($"Usuário com ID {idUsuario} não encontrado.");
            }

            var todosAlertasDaCidade = await _alertaRepo.GetByCityAsync(city);
            var alertasVisualizadosPeloUsuario = (await _visualizacaoRepo.GetVisualizacoesPorUsuarioAsync(idUsuario)).Select(v => v.AlertaId).ToHashSet();

            var alertasNaoVisualizados = todosAlertasDaCidade.Where(a => !alertasVisualizadosPeloUsuario.Contains(a.Id));
            
            return _mapper.Map<IEnumerable<AlertaDto>>(alertasNaoVisualizados);
        }

        public async Task<IEnumerable<AlertaDto>> GetByTipoAsync(string tipo)
        {
            var list = await _alertaRepo.GetByTipoAsync(tipo);
            return _mapper.Map<IEnumerable<AlertaDto>>(list);
        }

        public async Task<AlertaDto?> GetAlertaByIdAsync(int id)
        {
            var alerta = await _alertaRepo.GetByIdAsync(id);
            if (alerta == null)
            {
                return null;
            }
            return _mapper.Map<AlertaDto>(alerta);
        }

        public async Task<bool> DescartarAlertaParaUsuarioAsync(int alertaId, int idUsuario)
        {
            var alerta = await _alertaRepo.GetByIdAsync(alertaId);
            if (alerta == null)
            {
                throw new KeyNotFoundException($"Alerta com ID {alertaId} não encontrado.");
            }

            if (!await _usuarioRepo.ExistsAsync(idUsuario))
            {
                throw new KeyNotFoundException($"Usuário com ID {idUsuario} não encontrado.");
            }

            if (!await _visualizacaoRepo.ExisteVisualizacaoAsync(alertaId, idUsuario))
            {
                var descarte = new VisualizacaoAlerta
                {
                    AlertaId = alertaId,
                    UsuarioId = idUsuario,
                    DataDescarte = GetCurrentBrasiliaTime()
                };
                await _visualizacaoRepo.AdicionarVisualizacaoAsync(descarte);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAlertaAsync(int id)
        {
            var visualizacoes = await _visualizacaoRepo.GetVisualizacoesPorAlertaAsync(id);
            if (visualizacoes.Any())
            {
                _visualizacaoRepo.RemoverRangeVisualizacoes(visualizacoes);
            }
            return await _alertaRepo.DeleteAsync(id);
        }

        public async Task<int> DeleteTodosAlertasAsync()
        {
            var todosAlertas = await _alertaRepo.GetAllAsync();
            foreach(var alerta in todosAlertas)
            {
                var visualizacoes = await _visualizacaoRepo.GetVisualizacoesPorAlertaAsync(alerta.Id);
                if (visualizacoes.Any())
                {
                    _visualizacaoRepo.RemoverRangeVisualizacoes(visualizacoes);
                }
            }
            return await _alertaRepo.DeleteAllAsync();
        }

        public async Task<bool> DeleteAlertaPorOrigemAsync(int origemId, TipoOrigemAlerta tipoOrigem)
        {
            var alerta = await _alertaRepo.GetByOrigemAsync(origemId, tipoOrigem);
            if (alerta != null)
            {
                // Primeiro, remove as visualizações associadas a este alerta
                var visualizacoes = await _visualizacaoRepo.GetVisualizacoesPorAlertaAsync(alerta.Id);
                if (visualizacoes.Any())
                {
                    _visualizacaoRepo.RemoverRangeVisualizacoes(visualizacoes);
                }
                // Depois, remove o alerta
                return await _alertaRepo.DeleteAsync(alerta.Id);
            }
            return false; // Nenhum alerta encontrado para esta origem
        }

        private DateTime GetCurrentBrasiliaTime()
        {
            try
            {
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brasiliaTimeZone);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }
    }
} 