using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using DisasterLink_API.Utils; // Adicionada referência ao Utils
using DisasterLink_API.DTOs.Common; // Adicionado para LinkedResourceDto

namespace DisasterLink_API.DTOs // Ou DTOs.Response
{
    /// <summary>
    /// DTO para visualização de Abrigo Temporário
    /// </summary>
    public class AbrigoTemporarioDto : LinkedResourceDto // Modificado para herdar de LinkedResourceDto
    {
        /// <summary>
        /// ID do Abrigo Temporário
        /// </summary>
        [DefaultValue(101)]
        public int Id { get; set; }

        /// <summary>
        /// Nome do Abrigo Temporário
        /// Ex: "Abrigo Comunitário Esperança"
        /// </summary>
        [DefaultValue("Abrigo Comunitário Esperança")]
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do abrigo
        /// Ex: "Abrigo provisório montado na escola municipal. Oferece alimentação e pernoite."
        /// </summary>
        [DefaultValue("Abrigo provisório montado na escola municipal. Oferece alimentação e pernoite.")]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade/Município onde o abrigo está localizado
        /// Ex: "Nova Esperança"
        /// </summary>
        [DefaultValue("Nova Esperança")]
        public string CidadeMunicipio { get; set; } = null!;

        /// <summary>
        /// Bairro onde o abrigo está localizado
        /// Ex: "Feliz"
        /// </summary>
        [DefaultValue("Feliz")]
        public string Bairro { get; set; } = null!;

        /// <summary>
        /// Logradouro (Rua, Avenida, etc.) e número do abrigo
        /// Ex: "Rua da Escola, 500"
        /// </summary>
        [DefaultValue("Rua da Escola, 500")]
        public string Logradouro { get; set; } = null!;

        /// <summary>
        /// Capacidade total de pessoas do abrigo
        /// Ex: 50
        /// </summary>
        [DefaultValue(50)]
        public int Capacidade { get; set; }

        /// <summary>
        /// Lista de URLs das imagens do abrigo (pode ser nulo ou vazio)
        /// Ex: ["https://storage.disasterlink.com.br/abrigos/abrigo_esperanca_1.jpg"]
        /// </summary>
        public List<string>? ImagemUrls { get; set; }

        /// <summary>
        /// Data de cadastro do abrigo no sistema. Formato: yyyy-MM-ddTHH:mm:ss
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeJsonConverter))]
        public DateTime DataCadastro { get; set; }

        /// <summary>
        /// Status atual do abrigo (e.g., ativo, lotado, inativo)
        /// </summary>
        [DefaultValue("ativo")]
        public string Status { get; set; } = null!;
    }
} 