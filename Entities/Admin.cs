using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do administrador é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email do administrador é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [MaxLength(100, ErrorMessage = "O email não pode exceder 100 caracteres.")]
        public string Email { get; set; } = string.Empty; // Não editável após criação

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string SenhaHash { get; set; } = string.Empty; // Senha com hash

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
} 