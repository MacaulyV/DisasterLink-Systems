using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs.Auth
{
    public class GoogleLoginRequestDto
    {
        [Required(ErrorMessage = "O ID Token do Google é obrigatório.")]
        public string IdToken { get; set; } = string.Empty;
    }
} 