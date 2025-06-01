namespace DisasterLink_API.DTOs.ML
{
    public class CapacidadeResponseDto
    {
        public string CapacidadePrevista { get; set; } = string.Empty;
        public float[]? Scores { get; set; } // Opcional, para dar mais detalhes se necessário
    }
} 