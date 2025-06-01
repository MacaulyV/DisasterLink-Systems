using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DisasterLink_API.DTOs // Namespace pode ser DTOs.Create se preferir organizar assim
{
    /// <summary>
    /// DTO para criação de novo Abrigo Temporário
    /// </summary>
    public class AbrigoTemporarioCreateDto
    {
        /// <summary>
        /// Nome do Abrigo Temporário
        /// Ex: "Abrigo Comunitário Esperança"
        /// </summary>
        [Required(ErrorMessage = "O nome do abrigo é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome do abrigo pode ter no máximo 100 caracteres.")]
        [DefaultValue("Abrigo Comunitário Esperança")]
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Descrição detalhada do abrigo
        /// Ex: "Abrigo provisório montado na escola municipal. Oferece alimentação e pernoite."
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MaxLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        [DefaultValue("Abrigo provisório montado na escola municipal. Oferece alimentação e pernoite.")]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// Cidade/Município onde o abrigo está localizado
        /// Ex: "Nova Esperança"
        /// </summary>
        [Required(ErrorMessage = "A Cidade/Município é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A Cidade/Município pode ter no máximo 100 caracteres.")]
        [DefaultValue("Nova Esperança")]
        public string CidadeMunicipio { get; set; } = null!;

        /// <summary>
        /// Bairro onde o abrigo está localizado
        /// Ex: "Feliz"
        /// </summary>
        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O bairro pode ter no máximo 100 caracteres.")]
        [DefaultValue("Feliz")]
        public string Bairro { get; set; } = null!;

        /// <summary>
        /// Logradouro (Rua, Avenida, etc.) e número do abrigo
        /// Ex: "Rua da Escola, 500"
        /// </summary>
        [Required(ErrorMessage = "O logradouro é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O logradouro pode ter no máximo 200 caracteres.")]
        [DefaultValue("Rua da Escola, 500")]
        public string Logradouro { get; set; } = null!;

        /// <summary>
        /// Capacidade total de pessoas do abrigo
        /// Ex: 50
        /// </summary>
        [Required(ErrorMessage = "A capacidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser um número positivo.")]
        [DefaultValue(50)]
        public int Capacidade { get; set; }

        /// <summary>
        /// Lista de URLs das imagens do abrigo (opcional, máximo de 5 URLs)
        /// Cada URL deve ser válida
        /// Ex: ["https://storage.disasterlink.com.br/abrigos/abrigo_esperanca_1.jpg", "https://storage.disasterlink.com.br/abrigos/abrigo_esperanca_2.jpg"]
        /// </summary>
        [MaxLength(5, ErrorMessage = "A lista de imagens pode ter no máximo 5 URLs.")]
        // A validação de URL para cada item da lista pode ser feita no serviço ou com um custom ValidationAttribute se necessário
        // DefaultValue não é diretamente aplicável a listas complexas no Swagger da mesma forma, mas o exemplo no XML Doc ajuda
        public List<string>? ImagemUrls { get; set; }
    }
} 