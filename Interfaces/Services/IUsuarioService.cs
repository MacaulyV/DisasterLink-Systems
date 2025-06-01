using System.Threading.Tasks;
using DisasterLink_API.DTOs;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.Interfaces.Services
{
    /// <summary>
    /// Contrato de lógica de negócio para usuários
    /// </summary>
    public interface IUsuarioService
    {
        Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);
        Task<UsuarioDto?> GetByIdAsync(int id);
        Task UpdateAsync(int id, UsuarioUpdateDto dto);
        Task<LoginUsuarioResponseDto> LoginAsync(LoginDto dto);
        Task<List<UsuarioDto>> GetAllAsync();
        Task<List<UsuarioDto>> GetByLocalidadeAsync(string? cidadeMunicipio, string? bairro);
        Task DeleteAsync(int id);
        Task ForgotPasswordAsync(UsuarioForgotPasswordDto dto);
        Task<LoginUsuarioResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto dto);
    }
} 