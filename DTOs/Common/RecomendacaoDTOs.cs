using System.Collections.Generic;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// DTO para solicitação de recomendação de pontos de coleta
    /// </summary>
    public class SolicitacaoRecomendacaoDTO
    {
        /// <summary>
        /// Necessidade do usuário (ex: "alimentos", "fraldas", "medicamentos")
        /// </summary>
        public string? Necessidade { get; set; }

        /// <summary>
        /// Cidade para filtrar os resultados
        /// </summary>
        public string? Cidade { get; set; }
    }

    /// <summary>
    /// DTO para retorno de pontos de coleta recomendados
    /// </summary>
    public class PontoColetaRecomendadoDTO
    {
        /// <summary>
        /// Identificador do ponto de coleta
        /// </summary>
        public int PontoId { get; set; }

        /// <summary>
        /// Tipo do ponto de coleta (ex: "Alimentos", "Medicamentos")
        /// </summary>
        public string? Tipo { get; set; }

        /// <summary>
        /// Descrição do ponto de coleta
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Cidade onde o ponto de coleta está localizado
        /// </summary>
        public string? Cidade { get; set; }

        /// <summary>
        /// Bairro onde o ponto de coleta está localizado
        /// </summary>
        public string? Bairro { get; set; }

        /// <summary>
        /// Endereço completo do ponto de coleta
        /// </summary>
        public string? Logradouro { get; set; }

        /// <summary>
        /// Itens disponíveis no estoque
        /// </summary>
        public string? Estoque { get; set; }

        /// <summary>
        /// Lista de URLs das imagens do ponto de coleta
        /// </summary>
        public string? ImagemUrls { get; set; }

        /// <summary>
        /// Pontuação de relevância para a necessidade do usuário (quanto maior, mais relevante)
        /// </summary>
        public double Score { get; set; }
    }
} 