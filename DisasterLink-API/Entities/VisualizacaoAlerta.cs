using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    public class VisualizacaoAlerta
    {
        // Chave Primária Composta definida no DbContext
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int AlertaId { get; set; }
        public Alerta Alerta { get; set; } = null!;

        [Required]
        public DateTime DataDescarte { get; set; }
    }
} 