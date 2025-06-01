using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO para cadastro de novo usuário.
    /// </summary>
    public class UsuarioCreateDto
    {
        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [DefaultValue("Carlos Silva")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário. O campo email precisa conter o caractere '@'.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido. O email precisa conter o caractere '@'.")]
        [MaxLength(200, ErrorMessage = "O email pode ter no máximo 200 caracteres.")]
        [DefaultValue("carlos@exemplo.com")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha de acesso. Senha deve conter no mínimo 6 caracteres.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        [DefaultValue("654321")]
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// País do usuário (opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "O país pode ter no máximo 100 caracteres.")]
        [DefaultValue(null)]
        public string? Pais { get; set; }

        /// <summary>
        /// Estado do usuário (opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "O estado pode ter no máximo 100 caracteres.")]
        [DefaultValue(null)]
        public string? Estado { get; set; }

        /// <summary>
        /// Cidade/Município do usuário (opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "A cidade/município pode ter no máximo 100 caracteres.")]
        [DefaultValue(null)]
        public string? CidadeMunicipio { get; set; }

        /// <summary>
        /// Bairro do usuário (opcional).
        /// </summary>
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        [DefaultValue(null)]
        public string? Bairro { get; set; }
    }
} 