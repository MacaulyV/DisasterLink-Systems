using DisasterLink_API.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.Interfaces.Services
{
    public interface IAdminService
    {
        Task<AdminDto> GetByIdAsync(int id);
        Task<List<AdminDto>> GetAllAsync();
        Task<AdminDto> CreateAsync(AdminCreateDto adminCreateDto);
        Task<AdminLoginResponseDto> LoginAsync(AdminLoginDto adminLoginDto);
        Task UpdateAsync(int id, AdminUpdateDto adminUpdateDto);
        Task DeleteAsync(int id);
        Task ForgotPasswordAsync(AdminForgotPasswordDto dto);
    }
} 