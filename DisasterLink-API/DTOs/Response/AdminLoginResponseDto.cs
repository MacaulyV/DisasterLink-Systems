using System;

namespace DisasterLink_API.DTOs.Admin
{
    public class AdminLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public AdminDto Admin { get; set; } = null!; // Inicializado com null! para suprimir warning, será preenchido
        public DateTime Expiration { get; set; }
    }
} 