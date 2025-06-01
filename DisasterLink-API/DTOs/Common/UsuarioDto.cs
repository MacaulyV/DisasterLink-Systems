using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO de saída de usuário
    /// </summary>
    public class UsuarioDto : LinkedResourceDto
    {
        /// <summary>
        /// Identificador do usuário
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo
        /// </summary>
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// País do usuário
        /// </summary>
        public string? Pais { get; set; }

        /// <summary>
        /// Estado do usuário
        /// </summary>
        public string? Estado { get; set; }

        /// <summary>
        /// Cidade/Município do usuário
        /// </summary>
        public string? Municipio { get; set; }

        /// <summary>
        /// Bairro do usuário
        /// </summary>
        public string? Bairro { get; set; }
    }
} 