using AutoMapper;
using DisasterLink_API.Entities;
using DisasterLink_API.DTOs;
using DisasterLink_API.DTOs.Create;
using DisasterLink_API.DTOs.Response;
using DisasterLink_API.DTOs.Update;
using DisasterLink_API.DTOs.Admin;
using System.Text.Json;
using System.Collections.Generic;

namespace DisasterLink_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeamentos para AbrigoTemporario
            CreateMap<AbrigoTemporario, AbrigoTemporarioDto>()
                .ForMember(dest => dest.ImagemUrls, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.ImagemUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(src.ImagemUrls, (JsonSerializerOptions?)null) 
                    : null));

            CreateMap<AbrigoTemporarioCreateDto, AbrigoTemporario>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())       // Status é definido no serviço ao criar
                .ForMember(dest => dest.DataCadastro, opt => opt.Ignore())
                .ForMember(dest => dest.ImagemUrls, opt => opt.MapFrom(src => 
                    src.ImagemUrls != null && src.ImagemUrls.Count > 0 
                    ? JsonSerializer.Serialize(src.ImagemUrls, (JsonSerializerOptions?)null) 
                    : null));

            // Mapeamentos existentes (Alerta, Campanha, Participacao, Usuario, Admin)
            CreateMap<Alerta, AlertaDto>();
            CreateMap<AlertaCreateDto, Alerta>()
                .ForMember(dest => dest.DataHora, opt => opt.Ignore());
            CreateMap<Usuario, UsuarioDto>()
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.Pais))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.Municipio, opt => opt.MapFrom(src => src.Municipio))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Bairro));
            CreateMap<UsuarioCreateDto, Usuario>()
                .ForMember(dest => dest.Municipio, opt => opt.MapFrom(src => src.CidadeMunicipio))
                .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => src.Pais))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Bairro));
            CreateMap<UsuarioUpdateDto, Usuario>();

            CreateMap<Admin, AdminDto>()
                .ForMember(dest => dest.DataCriacaoValue, opt => opt.MapFrom(src => src.DataCriacao));
            CreateMap<AdminCreateDto, Admin>();

            // Mapeamentos para PontoDeColetaDeDoacoes
            CreateMap<PontoDeColetaDeDoacoes, PontoDeColetaDeDoacoesDto>()
                .ForMember(dest => dest.ImagemUrls, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.ImagemUrls) 
                    ? JsonSerializer.Deserialize<List<string>>(src.ImagemUrls, (JsonSerializerOptions?)null) 
                    : null));
            
            CreateMap<PontoDeColetaDeDoacoesCreateDto, PontoDeColetaDeDoacoes>()
                .ForMember(dest => dest.ImagemUrls, opt => opt.MapFrom(src => 
                    src.ImagemUrls != null && src.ImagemUrls.Count > 0 
                    ? JsonSerializer.Serialize(src.ImagemUrls, (JsonSerializerOptions?)null) 
                    : null));
            
            CreateMap<PontoDeColetaDeDoacoesUpdateDto, PontoDeColetaDeDoacoes>()
                .ForMember(d => d.Id, opt => opt.Ignore()) 
                .ForMember(d => d.DataInicio, opt => opt.Ignore()) 
                .ForMember(d => d.Participacoes, opt => opt.Ignore())
                .ForMember(dest => dest.ImagemUrls, opt => opt.MapFrom(src => 
                    src.ImagemUrls != null 
                        ? (src.ImagemUrls.Count > 0 ? JsonSerializer.Serialize(src.ImagemUrls, (JsonSerializerOptions?)null) : "[]") // Salva como JSON array vazio se a lista estiver vazia
                        : null)) // Se src.ImagemUrls for null, não mapeia (deixa o destino inalterado ou null)
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Mapeamentos para ParticipacaoPontoColeta
            CreateMap<ParticipacaoPontoColeta, ParticipacaoPontoColetaDto>();
            CreateMap<ParticipacaoPontoColetaCreateDto, ParticipacaoPontoColeta>()
                .ForMember(dest => dest.DataHora, opt => opt.Ignore());
        }
    }
} 