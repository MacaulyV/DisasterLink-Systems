namespace DisasterLink_API.DTOs
{
    public class LoginUsuarioResponseDto
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public UsuarioDto? Usuario { get; set; }

        public LoginUsuarioResponseDto(bool sucesso, string mensagem, UsuarioDto? usuario = null)
        {
            Sucesso = sucesso;
            Mensagem = mensagem;
            Usuario = usuario;
        }
    }
} 