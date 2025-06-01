using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    /// <summary>
    /// Define o tipo da entidade que originou o alerta.
    /// </summary>
    public enum TipoOrigemAlerta
    {
        PontoColeta,
        AbrigoTemporario,
        Manual // Caso o alerta não seja originado por uma entidade específica
    }

    /// <summary>
    /// Representa um alerta do sistema
    /// </summary>
    public class Alerta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        /// <summary>
        /// Tipo de alerta (enchente, deslizamento, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = null!;

        /// <summary>
        /// Título do alerta
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do alerta
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade alvo do alerta
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Cidade { get; set; } = null!;
        
        /// <summary>
        /// Bairro do alerta
        /// </summary>
        [MaxLength(100)]
        public string? Bairro { get; set; }
        
        /// <summary>
        /// Logradouro do alerta
        /// </summary>
        [MaxLength(200)]
        public string? Logradouro { get; set; }

        /// <summary>
        /// Data e hora de criação do alerta
        /// </summary>
        [Required]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// ID da entidade que originou o alerta (opcional).
        /// </summary>
        public int? OrigemId { get; set; }

        /// <summary>
        /// Tipo da entidade que originou o alerta (opcional).
        /// </summary>
        public TipoOrigemAlerta? TipoOrigem { get; set; }

        public ICollection<VisualizacaoAlerta> VisualizacoesAlerta { get; set; } = new List<VisualizacaoAlerta>();
    }
} 