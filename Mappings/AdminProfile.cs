using AutoMapper;
using DisasterLink_API.DTOs.Admin;
using DisasterLink_API.Entities;

namespace DisasterLink_API.Mappings
{
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            CreateMap<Admin, AdminDto>();
            CreateMap<AdminCreateDto, Admin>()
                .ForMember(dest => dest.SenhaHash, opt => opt.Ignore()); // SenhaHash será tratada no serviço
            
            // Não há mapeamento direto para AdminUpdateDto -> Admin aqui pois a lógica de atualização (especialmente senha)
            // será mais complexa e tratada no serviço.
        }
    }
} 