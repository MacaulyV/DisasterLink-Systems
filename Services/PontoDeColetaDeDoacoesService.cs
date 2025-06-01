using AutoMapper;
using DisasterLink_API.DTOs.Create;
using DisasterLink_API.DTOs.Response;
using DisasterLink_API.DTOs.Update;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.Services
{
    public class PontoDeColetaDeDoacoesService : IPontoDeColetaDeDoacoesService
    {
        private readonly IPontoDeColetaDeDoacoesRepository _pontoDeColetaRepository;
        private readonly IParticipacaoPontoColetaRepository _participacaoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;
        private readonly IAlertaService _alertaService;

        public PontoDeColetaDeDoacoesService(
            IPontoDeColetaDeDoacoesRepository pontoDeColetaRepository, 
            IParticipacaoPontoColetaRepository participacaoRepository, 
            IUsuarioRepository usuarioRepository, 
            IMapper mapper,
            IAlertaService alertaService)
        {
            _pontoDeColetaRepository = pontoDeColetaRepository;
            _participacaoRepository = participacaoRepository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
            _alertaService = alertaService;
        }

        private void ValidateImagemUrls(List<string>? imagemUrls)
        {
            if (imagemUrls != null)
            {
                if (imagemUrls.Count > 5)
                {
                    throw new ValidationException("A lista de imagens não pode conter mais que 5 URLs.");
                }
                foreach (var url in imagemUrls)
                {
                    if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                    {
                        throw new ValidationException($"A URL da imagem '{url}' não é válida.");
                    }
                }
            }
        }

        public async Task<PontoDeColetaDeDoacoesDto> CreatePontoDeColetaAsync(PontoDeColetaDeDoacoesCreateDto pontoDeColetaDto)
        {
            ValidateImagemUrls(pontoDeColetaDto.ImagemUrls);

            var pontoDeColeta = _mapper.Map<PontoDeColetaDeDoacoes>(pontoDeColetaDto);
            
            // Define a data de início como a data atual se não estiver definida
            if (pontoDeColeta.DataInicio == default)
            {
                pontoDeColeta.DataInicio = DateTime.UtcNow;
            }

            // O AddAsync no repositório irá gerar um ID aleatório de 4 dígitos (entre 1000-9999)
            var novoPonto = await _pontoDeColetaRepository.AddAsync(pontoDeColeta);
            
            // Mapeia para DTO e retorna com o ID gerado
            var novoPontoDto = _mapper.Map<PontoDeColetaDeDoacoesDto>(novoPonto);
            
            // Gerar alerta automático
            await _alertaService.CriarAlertaAutomaticoDePontoColetaAsync(novoPontoDto);
            
            return novoPontoDto;
        }

        public async Task<PontoDeColetaDeDoacoesDto?> UpdatePontoDeColetaAsync(int id, PontoDeColetaDeDoacoesUpdateDto pontoDeColetaDto)
        {
            ValidateImagemUrls(pontoDeColetaDto.ImagemUrls);

            var pontoExistente = await _pontoDeColetaRepository.GetByIdAsync(id);
            if (pontoExistente == null)
            {
                return null; // Ou lançar NotFoundException
            }

            var dataInicioOriginal = pontoExistente.DataInicio;

            _mapper.Map(pontoDeColetaDto, pontoExistente);

            // Restaurar DataInicio para garantir que não foi alterada pelo mapeamento
            pontoExistente.DataInicio = dataInicioOriginal;
            
            await _pontoDeColetaRepository.UpdateAsync(pontoExistente);
            return _mapper.Map<PontoDeColetaDeDoacoesDto>(pontoExistente);
        }

        public async Task<IEnumerable<PontoDeColetaDeDoacoesDto>> GetAllPontosDeColetaAsync(string? cidade, string? tipoDoacao)
        {
            var pontosDeColeta = await _pontoDeColetaRepository.GetAllAsync(cidade, tipoDoacao);
            return _mapper.Map<IEnumerable<PontoDeColetaDeDoacoesDto>>(pontosDeColeta);
        }

        public async Task<PontoDeColetaDeDoacoesDto?> GetPontoDeColetaByIdAsync(int id)
        {
            var pontoDeColeta = await _pontoDeColetaRepository.GetByIdAsync(id);
            if (pontoDeColeta == null)
            {
                return null;
            }
            return _mapper.Map<PontoDeColetaDeDoacoesDto>(pontoDeColeta);
        }

        public async Task<ParticipacaoPontoColetaDto?> AddParticipacaoAsync(int pontoColetaId, int idUsuario, ParticipacaoPontoColetaCreateDto participacaoDto)
        {
            // Validar se Ponto de Coleta existe
            if (!await _pontoDeColetaRepository.ExistsAsync(pontoColetaId))
            {
                // Lançar uma exceção ou retornar null indicando que o Ponto de Coleta não foi encontrado.
                // Por consistência com GetById, retornaremos null.
                // No controller, isso resultará em um 404 Not Found.
                // Alternativamente, poderia lançar: throw new KeyNotFoundException($"Ponto de coleta com ID {pontoColetaId} não encontrado.");
                return null; 
            }

            // Validar se Usuário existe
            if (!await _usuarioRepository.ExistsAsync(idUsuario)) // Assume que UsuarioRepository tem um método ExistsAsync
            {
                // Similar ao Ponto de Coleta, retornar null ou lançar exceção.
                // throw new KeyNotFoundException($"Usuário com ID {idUsuario} não encontrado.");
                return null;
            }

            var participacao = _mapper.Map<ParticipacaoPontoColeta>(participacaoDto);
            participacao.PontoColetaId = pontoColetaId;
            participacao.IdUsuario = idUsuario;

            // Define DataHora no fuso horário de Brasília
            try
            {
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                participacao.DataHora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brasiliaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback para UTC se o fuso horário de Brasília não for encontrado (improvável em sistemas Windows/Linux configurados)
                // Ou poderia lançar uma exceção de configuração.
                participacao.DataHora = DateTime.UtcNow; 
                // Considerar logar um aviso aqui.
            }
            
            // O repositório agora gera o ID da participação
            var novaParticipacao = await _participacaoRepository.AddAsync(participacao);
            return _mapper.Map<ParticipacaoPontoColetaDto>(novaParticipacao);
        }

        public async Task<IEnumerable<ParticipacaoPontoColetaDto>> GetParticipantesAsync(int pontoColetaId)
        {
            if (!await _pontoDeColetaRepository.ExistsAsync(pontoColetaId))
            {
                // Poderia retornar uma lista vazia ou lançar uma exceção NotFound
                return new List<ParticipacaoPontoColetaDto>(); 
            }
            var participantes = await _pontoDeColetaRepository.GetParticipantesAsync(pontoColetaId);
            return _mapper.Map<IEnumerable<ParticipacaoPontoColetaDto>>(participantes);
        }

        public async Task<bool> DeletePontoDeColetaAsync(int id)
        {
            var pontoExistente = await _pontoDeColetaRepository.GetByIdAsync(id);
            if (pontoExistente == null)
            {
                return false; // Não encontrado, não pode deletar
            }

            // Primeiro, tenta deletar o alerta associado
            // Não tratamos o resultado aqui, pois a exclusão do ponto deve prosseguir mesmo se o alerta não for encontrado/deletado
            await _alertaService.DeleteAlertaPorOrigemAsync(id, Entities.TipoOrigemAlerta.PontoColeta);

            // Em seguida, deleta as participações associadas (se houver)
            var participacoes = await _pontoDeColetaRepository.GetParticipantesAsync(id);
            if (participacoes.Any())
            {
                await _participacaoRepository.DeleteByPontoColetaIdAsync(id);
            }

            // Finalmente, deleta o ponto de coleta
            return await _pontoDeColetaRepository.DeleteAsync(id);
        }
    }
}