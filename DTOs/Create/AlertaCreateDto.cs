using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DisasterLink_API.Entities;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO para criação de novo alerta
    /// </summary>
    public class AlertaCreateDto
    {

                /// <summary>
        /// Tipo de alerta (enchente, deslizamento, etc.)
        /// </summary>
        [Required(ErrorMessage = "O tipo é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O tipo pode ter no máximo 50 caracteres.")]
        [DefaultValue("Inundação")]
        public string Tipo { get; set; } = null!;

        /// <summary>
        /// Título do alerta
        /// </summary>
        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O título pode ter no máximo 200 caracteres.")]
        [DefaultValue("ALERTA DE INUNDAÇÃO")]
        public string Titulo { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do alerta
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MaxLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        [DefaultValue("Previsão de chuvas intensas nas próximas 24h. Risco de inundação nas áreas baixas da cidade.")]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade alvo do alerta
        /// </summary>
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A cidade pode ter no máximo 100 caracteres.")]
        [DefaultValue("Porto Alegre")]
        public string Cidade { get; set; } = null!;
        
        /// <summary>
        /// Bairro do alerta (opcional)
        /// </summary>
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        [DefaultValue("Centro")]
        public string? Bairro { get; set; }
        
        /// <summary>
        /// Logradouro do alerta (opcional)
        /// </summary>
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        [DefaultValue("Avenida Borges de Medeiros, 1500")]
        public string? Logradouro { get; set; }

        /// <summary>
        /// ID da entidade que originou o alerta (opcional, usado internamente).
        /// Se não fornecido, será assumido como Manual.
        /// </summary>
        public int? OrigemId { get; set; }

        /// <summary>
        /// Tipo da entidade que originou o alerta (opcional, usado internamente).
        /// Se não fornecido, será assumido como Manual.
        /// </summary>
        public TipoOrigemAlerta? TipoOrigem { get; set; }
    }
} 