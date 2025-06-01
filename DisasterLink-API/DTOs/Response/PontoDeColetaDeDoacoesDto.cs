using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DisasterLink_API.Utils;
using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.DTOs.Response
{
    /// <summary>
    /// DTO para representar um Ponto de Coleta de Doações.
    /// </summary>
    public class PontoDeColetaDeDoacoesDto : LinkedResourceDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Descricao { get; set; } = null!;

        [JsonConverter(typeof(CustomDateTimeJsonConverter))]
        public DateTime DataInicio { get; set; }
        public string Cidade { get; set; } = null!;
        public string Bairro { get; set; } = null!;
        public string Logradouro { get; set; } = null!;
        public string? Estoque { get; set; }
        public List<string>? ImagemUrls { get; set; }
        public List<ParticipacaoPontoColetaDto> Participacoes { get; set; } = new List<ParticipacaoPontoColetaDto>();
    }
} 