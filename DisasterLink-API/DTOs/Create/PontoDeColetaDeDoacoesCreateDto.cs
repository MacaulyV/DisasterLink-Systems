using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs.Create
{
    /// <summary>
    /// DTO para criação de novo Ponto de Coleta de Doações.
    /// </summary>
    public class PontoDeColetaDeDoacoesCreateDto
    {
        /// <summary>
        /// Tipo/Propósito do ponto de coleta. Ex: "Arrecadação de alimentos"
        /// </summary>
        [Required(ErrorMessage = "O tipo é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O tipo pode ter no máximo 200 caracteres.")]
        [DefaultValue("Ponto de doação de água potável e itens de higiene")]
        public string Tipo { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do ponto de coleta.
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MaxLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        [DefaultValue("Coleta de cestas básicas para famílias afetadas.")]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade onde o ponto de coleta está localizado.
        /// </summary>
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A cidade pode ter no máximo 100 caracteres.")]
        [DefaultValue("Campinas")]
        public string Cidade { get; set; } = null!;

        /// <summary>
        /// Bairro onde o ponto de coleta está localizado.
        /// </summary>
        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        [DefaultValue("Jardim das Flores")]
        public string Bairro { get; set; } = null!;

        /// <summary>
        /// Logradouro (Rua, Avenida, etc.) e número do ponto de coleta.
        /// </summary>
        [Required(ErrorMessage = "O logradouro é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        [DefaultValue("Rua das Acácias, 100")]
        public string Logradouro { get; set; } = null!;

        /// <summary>
        /// Lista de URLs das imagens do ponto de coleta (opcional, máximo de 5 URLs).
        /// Cada URL deve ser válida.
        /// Ex: ["https://storage.disasterlink.com.br/pontos/ponto_coleta_1.jpg"]
        /// </summary>
        [MaxLength(5, ErrorMessage = "A lista de imagens pode ter no máximo 5 URLs.")]
        public List<string>? ImagemUrls { get; set; }

        /// <summary>
        /// Estoque/Meta do ponto de coleta (ex: "300 unidades", "1000 litros de água", "arroz, feijão"). Opcional.
        /// </summary>
        [MaxLength(200, ErrorMessage = "O estoque/meta pode ter no máximo 200 caracteres.")]
        [DefaultValue("Arroz, feijão, óleo")]
        public string? Estoque { get; set; }
    }
} 