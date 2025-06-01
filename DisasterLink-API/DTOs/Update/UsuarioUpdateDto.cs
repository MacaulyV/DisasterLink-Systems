using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO para atualização de perfil de usuário
    /// </summary>
    public class UsuarioUpdateDto
    {
        /// <summary>
        /// Nome completo
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        // Email não pode ser alterado conforme especificação de "Endpoints da entidade Usuario (Ajustados)"
        // Se precisar ser alterável, descomente e ajuste as regras no serviço.
        // /// <summary>
        // /// Email do usuário (não pode ser alterado após o cadastro inicial)
        // /// </summary>
        // [EmailAddress(ErrorMessage = "Email inválido.")]
        // [MaxLength(200, ErrorMessage = "O email pode ter no máximo 200 caracteres.")]
        // public string? Email { get; set; }

        /// <summary>
        /// Senha atual do usuário (obrigatória se for alterar a senha)
        /// </summary>
        public string? SenhaAtual { get; set; }

        /// <summary>
        /// Nova senha (deve atender aos critérios de complexidade se fornecida)
        /// </summary>
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        // Adicionar RegularExpression se houver requisitos de complexidade específicos para usuário comum além do tamanho.
        public string? NovaSenha { get; set; }

        /// <summary>
        /// Município do usuário (opcional para atualização)
        /// </summary>
        [MaxLength(100, ErrorMessage = "O município pode ter no máximo 100 caracteres.")]
        public string? Municipio { get; set; }
    }
} 