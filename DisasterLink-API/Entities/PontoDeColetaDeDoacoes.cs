using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    /// <summary>
    /// Representa um ponto de coleta de doações.
    /// </summary>
    public class PontoDeColetaDeDoacoes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // O ID será gerado manualmente com 4 dígitos
        public int Id { get; set; }

        /// <summary>
        /// Tipo/Propósito do ponto de coleta. Ex: "Arrecadação de roupas", "Ponto de água potável"
        /// </summary>
        [Required(ErrorMessage = "O tipo do ponto de coleta é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O tipo pode ter no máximo 200 caracteres.")]
        public string Tipo { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do ponto de coleta.
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MaxLength(1000, ErrorMessage = "A descrição pode ter no máximo 1000 caracteres.")]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Data de início da coleta (formato yyyy-MM-dd no payload).
        /// </summary>
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; }

        /// <summary>
        /// Cidade onde o ponto de coleta está localizado.
        /// </summary>
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A cidade pode ter no máximo 100 caracteres.")]
        public string Cidade { get; set; } = null!;

        /// <summary>
        /// Bairro onde o ponto de coleta está localizado.
        /// </summary>
        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        public string Bairro { get; set; } = null!;

        /// <summary>
        /// Logradouro (Rua, Avenida, etc.) e número do ponto de coleta.
        /// </summary>
        [Required(ErrorMessage = "O logradouro é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        public string Logradouro { get; set; } = null!;

        /// <summary>
        /// Estoque/Meta do ponto de coleta (ex: "300 unidades", "1000 litros de água"). Opcional.
        /// </summary>
        [MaxLength(200, ErrorMessage = "O estoque/meta pode ter no máximo 200 caracteres.")]
        public string? Estoque { get; set; }

        /// <summary>
        /// Lista de URLs das imagens do ponto de coleta, armazenada como string JSON.
        /// </summary>
        // Não há DataAnnotation para MaxLength de string representando JSON aqui, pois a validação da contagem de URLs
        // e do formato das URLs será feita na camada de serviço antes do mapeamento para esta string.
        // A string pode ser longa dependendo do número de URLs e seus comprimentos.
        public string? ImagemUrls { get; set; } // Armazena como JSON string: ["url1", "url2"]

        /// <summary>
        /// Participações no ponto de coleta.
        /// </summary>
        public ICollection<ParticipacaoPontoColeta> Participacoes { get; set; } = new List<ParticipacaoPontoColeta>();
    }
} 