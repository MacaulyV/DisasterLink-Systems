using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO para login de usuário ou administrador.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email do usuário ou administrador. O email precisa conter o caractere '@'.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido. O email precisa conter o caractere '@'.")]
        [DefaultValue("carlos@exemplo.com")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Senha do usuário ou administrador. Senha deve conter no mínimo 6 caracteres.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DefaultValue("654321")]
        public string Senha { get; set; } = null!;
    }
} 