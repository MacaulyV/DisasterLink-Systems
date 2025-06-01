using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs.Auth
{
    public class AdminForgotPasswordDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da nova senha é obrigatória.")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação da nova senha não corresponde.")]
        public string ConfirmacaoNovaSenha { get; set; } = string.Empty;
    }
} 