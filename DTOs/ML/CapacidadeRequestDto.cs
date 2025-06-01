using System.ComponentModel.DataAnnotations;

namespace DisasterLink_API.DTOs.ML
{
    public class CapacidadeRequestDto
    {
        [Required(ErrorMessage = "O tipo do ponto de coleta é obrigatório.")]
        public string TipoPontoColeta { get; set; } = string.Empty;

        [Required(ErrorMessage = "A contagem de itens em estoque é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A contagem de itens deve ser um número não negativo.")]
        public int ContagemItensEstoque { get; set; }
    }
} 