namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO de resposta de login
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Identificador do usuário
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Placeholder para token (JWT a implementar)
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
} 