using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    /// <summary>
    /// Representa a participação de um usuário em um ponto de coleta de doações.
    /// </summary>
    public class ParticipacaoPontoColeta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // ID será gerado manualmente (4 dígitos)
        public int Id { get; set; }

        /// <summary>
        /// ID do Ponto de Coleta ao qual esta participação está vinculada.
        /// </summary>
        [Required(ErrorMessage = "O ID do ponto de coleta é obrigatório.")]
        public int PontoColetaId { get; set; }

        /// <summary>
        /// ID do Usuário que está participando.
        /// </summary>
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Forma de ajuda oferecida (ex: "Doação de alimentos", "Trabalho voluntário").
        /// </summary>
        [Required(ErrorMessage = "A forma de ajuda é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A forma de ajuda pode ter no máximo 100 caracteres.")]
        public string FormaDeAjuda { get; set; } = null!;

        /// <summary>
        /// Mensagem adicional do participante (opcional).
        /// </summary>
        [MaxLength(1000, ErrorMessage = "A mensagem pode ter no máximo 1000 caracteres.")]
        public string? Mensagem { get; set; }

        /// <summary>
        /// Contato (email) do participante (opcional).
        /// </summary>
        [MaxLength(200, ErrorMessage = "O contato (email) pode ter no máximo 200 caracteres.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido para o contato.")]
        public string? Contato { get; set; }

        /// <summary>
        /// Telefone de contato do participante (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [MaxLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
        public string Telefone { get; set; } = null!;

        /// <summary>
        /// Data e hora do registro da participação (Horário de Brasília GMT-3).
        /// </summary>
        [Required]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Ponto de Coleta de Doações relacionado.
        /// </summary>
        [ForeignKey("PontoColetaId")]
        public PontoDeColetaDeDoacoes PontoDeColetaDeDoacoes { get; set; } = null!;

        // Navegação para Usuario pode ser adicionada aqui, se necessário.
        // Ex: public Usuario Usuario { get; set; }
    }
} 