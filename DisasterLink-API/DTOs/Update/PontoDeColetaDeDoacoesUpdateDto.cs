using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs.Update
{
    /// <summary>
    /// DTO para atualização de um Ponto de Coleta de Doações.
    /// </summary>
    public class PontoDeColetaDeDoacoesUpdateDto
    {
        /// <summary>
        /// Novo tipo/propósito do ponto de coleta. Ex: "Arrecadação de roupas"
        /// </summary>
        [MaxLength(200, ErrorMessage = "O tipo pode ter no máximo 200 caracteres.")]
        public string? Tipo { get; set; }

        /// <summary>
        /// Nova descrição detalhada do ponto de coleta.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        public string? Descricao { get; set; }

        // DataInicio não pode ser atualizada via PATCH, conforme especificado.

        /// <summary>
        /// Nova cidade onde o ponto de coleta está localizado.
        /// </summary>
        [MaxLength(100, ErrorMessage = "A cidade pode ter no máximo 100 caracteres.")]
        public string? Cidade { get; set; }

        /// <summary>
        /// Novo bairro onde o ponto de coleta está localizado.
        /// </summary>
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        public string? Bairro { get; set; }

        /// <summary>
        /// Novo logradouro (Rua, Avenida, etc.) e número do ponto de coleta.
        /// </summary>
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        public string? Logradouro { get; set; }

        /// <summary>
        /// Nova lista de URLs das imagens do ponto de coleta (opcional, máximo de 5 URLs).
        /// Enviar uma lista vazia para remover todas as imagens ou null para não alterar.
        /// </summary>
        [MaxLength(5, ErrorMessage = "A lista de imagens pode ter no máximo 5 URLs.")]
        public List<string>? ImagemUrls { get; set; }

        /// <summary>
        /// Novo estoque/meta do ponto de coleta (ex: "300 unidades", "1000 litros de água"). Opcional.
        /// </summary>
        [MaxLength(200, ErrorMessage = "O estoque/meta pode ter no máximo 200 caracteres.")]
        public string? Estoque { get; set; }
    }
} 