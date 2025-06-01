using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs.Admin
{
    public class AdminUpdateDto
    {
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "A senha atual é obrigatória para realizar alterações.")]
        public string SenhaAtual { get; set; } = string.Empty;

        // Email não pode ser alterado, então não está aqui.

        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        public string? NovaSenha { get; set; }

        [Compare("NovaSenha", ErrorMessage = "As novas senhas não coincidem.")]
        public string? ConfirmacaoNovaSenha { get; set; }
    }
} 