using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs.Admin
{
    /// <summary>
    /// DTO para login de administrador.
    /// </summary>
    public class AdminLoginDto
    {
        /// <summary>
        /// Email do administrador. O email precisa conter o caractere '@'.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido. O email precisa conter o caractere '@'.")]
        [DefaultValue("luciana@admin.com")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do administrador. Senha deve conter no mínimo 6 caracteres.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DefaultValue("985214")]
        public string Senha { get; set; } = string.Empty;
    }
} 