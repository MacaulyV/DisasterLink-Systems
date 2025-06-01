using System;
using System.Text.Json.Serialization;
using DisasterLink_API.Utils;
using DisasterLink_API.Entities;
using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO de saída de alerta
    /// </summary>
    public class AlertaDto : LinkedResourceDto
    {
        /// <summary>
        /// Identificador do alerta
        /// </summary>
        [JsonPropertyOrder(-20)] // Garante que Id seja o primeiro
        public int Id { get; set; }

        /// <summary>
        /// Tipo de alerta
        /// </summary>
        [JsonPropertyOrder(-10)] // Garante que Tipo seja o segundo
        public string Tipo { get; set; } = null!;

        /// <summary>
        /// Título do alerta
        /// </summary>
        public string Titulo { get; set; } = null!;

        /// <summary>
        /// Descrição do alerta
        /// </summary>
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade alvo do alerta
        /// </summary>
        public string Cidade { get; set; } = null!;
        
        /// <summary>
        /// Bairro do alerta
        /// </summary>
        public string? Bairro { get; set; }
        
        /// <summary>
        /// Logradouro do alerta
        /// </summary>
        public string? Logradouro { get; set; }

        /// <summary>
        /// Data e hora de criação
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeJsonConverter))]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// ID da entidade que originou o alerta (opcional).
        /// </summary>
        public int? OrigemId { get; set; }

        /// <summary>
        /// Tipo da entidade que originou o alerta (opcional).
        /// </summary>
        public TipoOrigemAlerta? TipoOrigem { get; set; }
    }
} 