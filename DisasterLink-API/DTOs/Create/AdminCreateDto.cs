using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs.Admin
{
    /// <summary>
    /// DTO para cadastro de novo administrador.
    /// </summary>
    public class AdminCreateDto
    {
        /// <summary>
        /// Nome completo do administrador.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [DefaultValue("Luciana Admin")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do administrador. Email deve conter '@'.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido. O email precisa conter o caractere '@'.")]
        [MaxLength(100, ErrorMessage = "O email pode ter no máximo 100 caracteres.")]
        [DefaultValue("luciana@admin.com")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha de acesso. Senha deve conter no mínimo 6 caracteres.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DefaultValue("985214")]
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da senha. Confirmação de senha é obrigatória e deve ser idêntica à senha informada.
        /// </summary>
        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem. A confirmação de senha deve ser idêntica à senha informada.")]
        [DefaultValue("985214")]
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }
} 