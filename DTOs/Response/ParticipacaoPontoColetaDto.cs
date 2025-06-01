using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DisasterLink_API.Utils;

namespace DisasterLink_API.DTOs.Response
{
    /// <summary>
    /// DTO para representar uma participação em Ponto de Coleta de Doações.
    /// </summary>
    public class ParticipacaoPontoColetaDto
    {
        public int Id { get; set; }
        public int PontoColetaId { get; set; }
        public int IdUsuario { get; set; }
        public string FormaDeAjuda { get; set; } = null!;
        public string? Mensagem { get; set; }
        public string? Contato { get; set; } // Opcional
        public string Telefone { get; set; } = null!;

        [JsonConverter(typeof(CustomDateTimeJsonConverter))]
        public DateTime DataHora { get; set; }
    }
} 