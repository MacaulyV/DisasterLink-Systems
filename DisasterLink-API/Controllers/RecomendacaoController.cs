using DisasterLink_API.DTOs;
using DisasterLink_API.Interfaces;
using DisasterLink_API.MLModels;
using DisasterLink_API.SwaggerExamples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecomendacaoController : ControllerBase
    {
        private readonly IRecomendacaoPontoColetaService _recomendacaoService;

        public RecomendacaoController(IRecomendacaoPontoColetaService recomendacaoService)
        {
            _recomendacaoService = recomendacaoService;
        }

        /// <summary>
        /// Busca pontos de coleta usando um modelo de Machine Learning feito com (ML.NET).
        /// </summary>
        /// <remarks>
        /// Este serviço usa um modelo de Machine Learning (ML.NET) para encontrar os pontos de coleta mais adequados
        /// baseado em dois critérios principais:
        /// 
        /// 1. Necessidade do item:
        ///    - Alimentos
        ///    - Roupas
        ///    - Kits de higiene
        ///    - Água potável
        ///    - Fraldas
        ///    - Medicamentos
        /// 
        /// 2. Localização do solicitante
        /// 
        /// O sistema analisa esses dados e retorna uma LISTA de pontos de coleta recomendados,
        /// cada um com uma pontuação (score) que indica o quão relevante é a recomendação.
        /// 
        /// Exemplo de uso:
        /// - Se você precisa de alimentos em São Paulo, o sistema vai retornar vários pontos de coleta
        ///   que têm alimentos disponíveis, ordenados do mais adequado para o menos adequado,
        ///   considerando fatores como proximidade e disponibilidade de estoque.
        /// 
        /// Observação: O modelo de Machine Learning é treinado periodicamente com dados atualizados
        /// para garantir recomendações precisas e relevantes.
        /// </remarks>
        /// <param name="solicitacao">Objeto contendo a necessidade e a localização do solicitante</param>
        /// <returns>Lista de pontos de coleta recomendados, com suas respectivas pontuações de relevância</returns>
        [HttpPost("pontos-coleta")]
        [SwaggerRequestExample(typeof(SolicitacaoRecomendacaoDTO), typeof(SolicitacaoRecomendacaoExample))]
        public async Task<ActionResult<List<PontoColetaRecomendadoDTO>>> ObterRecomendacoes([FromBody] SolicitacaoRecomendacaoDTO solicitacao)
        {
            if (string.IsNullOrWhiteSpace(solicitacao.Necessidade))
            {
                return BadRequest("O campo 'necessidade' é obrigatório.");
            }

            var resultados = await _recomendacaoService.ObterRecomendacoesAsync(
                solicitacao.Necessidade,
                solicitacao.Cidade);

            if (resultados == null || !resultados.Any())
            {
                return NotFound("Não foram encontrados pontos de coleta para esta necessidade e localização.");
            }

            var recomendacoes = resultados.Select(r => new PontoColetaRecomendadoDTO
            {
                PontoId = r.PontoId,
                Tipo = r.Tipo,
                Descricao = r.Descricao,
                Cidade = r.Cidade,
                Bairro = r.Bairro,
                Logradouro = r.Logradouro,
                Estoque = r.Estoque,
                ImagemUrls = r.ImagemUrls,
                Score = r.Score
            }).ToList();

            return Ok(recomendacoes);
        }

        /// <summary>
        /// Retorna o ponto de coleta mais adequado para atender uma necessidade específica.
        /// </summary>
        /// <remarks>
        /// Como funciona:
        /// 
        /// 1. O sistema recebe:
        ///    - A necessidade do solicitante (ex: alimentos, roupas, medicamentos)
        ///    - A localização do solicitante (cidade)
        /// 
        /// 2. O sistema analisa:
        ///    - Disponibilidade de estoque nos pontos de coleta
        ///    - Proximidade geográfica
        ///    - Histórico de doações
        ///    - Avaliações dos pontos
        /// 
        /// 3. O resultado:
        ///    - Retorna APENAS o ponto de coleta mais adequado
        ///    - Inclui uma pontuação (score) que indica o quão relevante é a recomendação
        ///    - Quanto maior o score, melhor a recomendação
        /// 
        /// Exemplo prático:
        /// Se você precisa de alimentos em São Paulo, o sistema vai retornar o ponto de coleta
        /// que tem a melhor combinação de: alimentos disponíveis + proximidade + boa avaliação.
        /// </remarks>
        /// <param name="solicitacao">Dados da solicitação (necessidade e localização)</param>
        /// <returns>O ponto de coleta mais recomendado com sua pontuação de relevância</returns>
        [HttpPost("melhor-ponto-coleta")]
        [SwaggerRequestExample(typeof(SolicitacaoRecomendacaoDTO), typeof(SolicitacaoRecomendacaoExample))]
        public async Task<ActionResult<PontoColetaRecomendadoDTO>> ObterMelhorRecomendacao([FromBody] SolicitacaoRecomendacaoDTO solicitacao)
        {
            if (string.IsNullOrWhiteSpace(solicitacao.Necessidade))
            {
                return BadRequest("O campo 'necessidade' é obrigatório.");
            }

            var resultados = await _recomendacaoService.ObterRecomendacoesAsync(
                solicitacao.Necessidade,
                solicitacao.Cidade);

            if (resultados == null || !resultados.Any())
            {
                return NotFound("Não foram encontrados pontos de coleta para esta necessidade e localização.");
            }

            // O primeiro item da lista já é o que tem o maior score devido à ordenação no serviço
            var melhorResultado = resultados.First(); 

            var recomendacao = new PontoColetaRecomendadoDTO
            {
                PontoId = melhorResultado.PontoId,
                Tipo = melhorResultado.Tipo,
                Descricao = melhorResultado.Descricao,
                Cidade = melhorResultado.Cidade,
                Bairro = melhorResultado.Bairro,
                Logradouro = melhorResultado.Logradouro,
                Estoque = melhorResultado.Estoque,
                ImagemUrls = melhorResultado.ImagemUrls,
                Score = melhorResultado.Score
            };

            return Ok(recomendacao);
        }

        /// <summary>
        /// Retreina o modelo de Machine Learning (ML.NET) com dados atualizados. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar o modelo de recomendação com os dados mais recentes:
        /// 
        /// 1. O que faz:
        ///    - Coleta todos os pontos de coleta atuais do banco de dados
        ///    - Processa os dados para treinar um novo modelo ML.NET
        ///    - Salva o modelo treinado como arquivo .zip
        /// 
        /// 2. Onde salva:
        ///    - Pasta: MLModels/
        ///    - Formato: arquivo .zip
        /// 
        /// 3. Importante:
        ///    - Apenas administradores podem executar esta operação
        ///    - O processo pode levar alguns minutos dependendo da quantidade de dados
        ///    - O modelo anterior é substituído pelo novo
        /// </remarks>
        /// <returns>Mensagem confirmando o sucesso do treinamento</returns>
        [HttpPost("treinar-modelo")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TreinarModelo()
        {
            await _recomendacaoService.AtualizarModeloAsync();
            return Ok("Modelo de Machine Learning treinado e salvo com sucesso.");
        }
    }
} 