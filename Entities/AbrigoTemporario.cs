using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterLink_API.Entities
{
    public class AbrigoTemporario
    {
        [Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Mantido comentado/removido para geração pela API
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do abrigo é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome do abrigo pode ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MaxLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        public string Descricao { get; set; } = null!;

        [Required(ErrorMessage = "A Cidade/Município é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A Cidade/Município pode ter no máximo 100 caracteres.")]
        public string CidadeMunicipio { get; set; } = null!;

        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        public string Bairro { get; set; } = null!;

        [Required(ErrorMessage = "O logradouro é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        public string Logradouro { get; set; } = null!;

        [Required(ErrorMessage = "A capacidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser um número positivo.")]
        public int Capacidade { get; set; }

        // ImagemUrls será uma string JSON serializada no banco de dados.
        // A lógica de serialização/desserialização e validação (limite de 5) será tratada no serviço/mapeamento.
        public string? ImagemUrls { get; set; } // Armazena como JSON string: ["url1", "url2"]

        [Required]
        public DateTime DataCadastro { get; set; }

        [Required]
        [MaxLength(20)] // Ex: "ativo", "lotado", "inativo"
        public string Status { get; set; } = "ativo";
    }
} 