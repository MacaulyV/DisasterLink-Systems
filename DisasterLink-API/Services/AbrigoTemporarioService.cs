using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DisasterLink_API.DTOs;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Interfaces.Services;
using System.ComponentModel.DataAnnotations; // Para ValidationException

namespace DisasterLink_API.Services
{
    /// <summary>
    /// Implementa lógica de negócio para Abrigos Temporários
    /// </summary>
    public class AbrigoTemporarioService : IAbrigoTemporarioService
    {
        private readonly IAbrigoTemporarioRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAlertaService _alertaService;

        public AbrigoTemporarioService(IAbrigoTemporarioRepository repo, IMapper mapper, IAlertaService alertaService)
        {
            _repo = repo;
            _mapper = mapper;
            _alertaService = alertaService;
        }

        public async Task<AbrigoTemporarioDto> CreateAsync(AbrigoTemporarioCreateDto dto, int usuarioId) // usuarioId pode ser usado para auditoria
        {
            // Validação para ImagemUrls
            if (dto.ImagemUrls != null)
            {
                if (dto.ImagemUrls.Count > 5)
                {
                    throw new ValidationException("A lista de imagens não pode conter mais que 5 URLs.");
                }
                foreach (var url in dto.ImagemUrls)
                {
                    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                    {
                        throw new ValidationException($"A URL da imagem '{url}' não é válida.");
                    }
                }
            }

            var entity = _mapper.Map<AbrigoTemporario>(dto);
            try
            {
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                entity.DataCadastro = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brasiliaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                entity.DataCadastro = DateTime.UtcNow; 
            }
            entity.Status = "ativo"; // Padrão inicial
            // VagasDisponiveis é definida através do DTO agora.
            // Se VagasDisponiveis não estivesse no DTO, seria inicializada aqui (ex: entity.VagasDisponiveis = entity.Capacidade;)
            
            var created = await _repo.AddAsync(entity);
            // Log de criação com usuarioId pode ser adicionado aqui
            
            // Converter para DTO para retornar
            var createdDto = _mapper.Map<AbrigoTemporarioDto>(created);
            
            // Gerar alerta automático
            await _alertaService.CriarAlertaAutomaticoDeAbrigoAsync(createdDto);
            
            return createdDto;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing is null)
                throw new KeyNotFoundException("Abrigo temporário não encontrado.");

            // Tenta deletar o alerta associado
            await _alertaService.DeleteAlertaPorOrigemAsync(id, Entities.TipoOrigemAlerta.AbrigoTemporario);

            await _repo.DeleteAsync(existing);
        }

        public async Task<IEnumerable<AbrigoTemporarioDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(o => _mapper.Map<AbrigoTemporarioDto>(o));
        }

        public async Task<AbrigoTemporarioDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity is null ? null : _mapper.Map<AbrigoTemporarioDto>(entity);
        }

        public async Task<IEnumerable<AbrigoTemporarioDto>> GetByCityAsync(string city)
        {
            var list = await _repo.GetByCityAsync(city);
            return list.Select(o => _mapper.Map<AbrigoTemporarioDto>(o));
        }

        public async Task UpdateAsync(int id, AbrigoTemporarioCreateDto dto) // Idealmente, usar um AbrigoTemporarioUpdateDto aqui
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing is null)
                throw new KeyNotFoundException("Abrigo temporário não encontrado.");

            // Validação para ImagemUrls na atualização
            if (dto.ImagemUrls != null)
            {
                if (dto.ImagemUrls.Count > 5)
                {
                    throw new ValidationException("A lista de imagens não pode conter mais que 5 URLs.");
                }
                foreach (var url in dto.ImagemUrls)
                {
                    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                    {
                        throw new ValidationException($"A URL da imagem '{url}' não é válida.");
                    }
                }
            }

            _mapper.Map(dto, existing); // Usando AutoMapper para atualizar os campos
            // Não se deve atualizar DataCadastro aqui.
            // O Status e VagasDisponiveis são atualizados via DTO.

            await _repo.UpdateAsync(existing);
        }
    }
} 