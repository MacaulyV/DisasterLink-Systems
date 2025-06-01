using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs.Create
{
    /// <summary>
    /// DTO com dados para registrar a participação de um usuário em um ponto de coleta.
    /// </summary>
    public class ParticipacaoPontoColetaCreateDto
    {
        /// <summary>
        /// Forma de ajuda que será oferecida (ex: "Doação de alimentos", "Trabalho voluntário").
        /// </summary>
        [Required(ErrorMessage = "A forma de ajuda é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A forma de ajuda pode ter no máximo 100 caracteres.")]
        [DefaultValue("Doação de roupas e agasalhos")]
        public string FormaDeAjuda { get; set; } = null!;

        /// <summary>
        /// Mensagem adicional do participante (opcional).
        /// </summary>
        [MaxLength(1000, ErrorMessage = "A mensagem pode ter no máximo 1000 caracteres.")]
        [DefaultValue("Entregarei os itens na próxima semana.")]
        public string? Mensagem { get; set; }

        /// <summary>
        /// Contato (email) do participante (opcional).
        /// </summary>
        [MaxLength(200, ErrorMessage = "O contato (email) pode ter no máximo 200 caracteres.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido para o contato.")]
        [DefaultValue("participante@email.com")]
        public string? Contato { get; set; }

        /// <summary>
        /// Telefone de contato do participante (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [MaxLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
        [DefaultValue("(XX) 9XXXX-XXXX")]
        public string Telefone { get; set; } = null!;
    }
} 