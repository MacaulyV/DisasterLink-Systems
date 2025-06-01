using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? SenhaHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? GoogleUserId { get; set; }

        [MaxLength(100)]
        public string? Pais { get; set; }

        [MaxLength(100)]
        public string? Estado { get; set; }

        [MaxLength(100)]
        public string? Municipio { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        // public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>(); // Removido
        public ICollection<VisualizacaoAlerta> VisualizacoesAlerta { get; set; } = new List<VisualizacaoAlerta>();
    }
} 