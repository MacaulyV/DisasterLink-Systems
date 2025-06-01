using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DisasterLink_API.DTOs;
using DisasterLink_API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertasController : ControllerBase
    {
        private readonly IAlertaService _service;

        public AlertasController(IAlertaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna todos os alertas ativos do sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar todos os alertas que estão atualmente ativos no sistema.
        /// 
        /// Características:
        /// - Requer autenticação JWT para acesso
        /// - Acesso restrito apenas para administradores
        /// - Retorna uma lista completa de alertas ativos
        /// 
        /// Exemplo de uso:
        /// GET /api/alertas/ativos
        /// 
        /// 🔒 Segurança: Este endpoint é protegido e requer autenticação JWT válida
        /// </remarks>
        [HttpGet("ativos")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AlertaDto>), 200)]
        public async Task<ActionResult<IEnumerable<AlertaDto>>> GetAtivos()
        {
            var list = await _service.GetAllAsync();
            foreach (var alerta in list)
            {
                AddLinksToAlertaDto(alerta);
            }
            return Ok(list);
        }

            /// <summary>
            /// Busca alertas ativos para uma cidade específica, filtrando apenas aqueles que o usuário ainda não visualizou.
            /// </summary>
            /// <remarks>
            /// Este endpoint permite consultar alertas ativos para uma cidade específica, filtrando
            /// apenas aqueles que o usuário ainda não visualizou.
            /// 
            /// Características:
            /// - Acesso público (não requer autenticação)
            /// - Filtra alertas por cidade
            /// - Mostra apenas alertas não visualizados pelo usuário
            /// - Requer ID do usuário válido
            /// </remarks>
            /// <param name="nomeCidade">Nome da cidade para filtrar os alertas</param>
            /// <param name="idUsuario">ID do usuário para filtrar alertas não visualizados</param>
            /// <response code="200">Retorna lista de alertas ativos não visualizados</response>
            /// <response code="400">Retornado quando o ID do usuário é inválido</response>
            /// <response code="404">Retornado quando o usuário não é encontrado</response>
        [HttpGet("cidade")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<AlertaDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<ActionResult<IEnumerable<AlertaDto>>> GetByCity([FromQuery] string nomeCidade, [FromQuery] int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new ProblemDetails { Title = "ID de Usuário inválido", Detail = "O parâmetro idUsuario é obrigatório e deve ser um número positivo." });
            }
            try
            {
                var list = await _service.GetByCityAsync(nomeCidade, idUsuario);
                foreach (var alerta in list)
                {
                    AddLinksToAlertaDto(alerta, idUsuario);
                }
                return Ok(list);
            }
            catch (KeyNotFoundException ex) 
            {
                return NotFound(new ProblemDetails { Title = "Usuário não encontrado", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos os alertas de um determinado tipo. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar todos os alertas de um tipo específico no sistema.
        /// 
        /// Características:
        /// - Requer autenticação via token JWT
        /// - Retorna todos os alertas do tipo solicitado
        /// - Não considera histórico de visualizações dos usuários
        /// 
        /// Diferenças em relação ao endpoint /alertas/cidade:
        /// - Não filtra por cidade
        /// - Não considera alertas já visualizados
        /// - Retorna todos os alertas do tipo, independente do status
        /// 
        /// Exemplo de uso:
        /// GET /api/alertas/tipo/emergencia
        /// 
        /// Observações:
        /// - Token JWT deve ser enviado no header da requisição
        /// - Retorna todos os alertas do tipo, mesmo que já visualizados
        /// </remarks>
        /// <param name="tipo">Tipo de alerta a ser consultado (ex: emergencia, informativo, etc)</param>
        [HttpGet("tipo/{tipo}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AlertaDto>), 200)]
        public async Task<ActionResult<IEnumerable<AlertaDto>>> GetByTipo(string tipo)
        {
            var list = await _service.GetByTipoAsync(tipo);
            foreach (var alerta in list)
            {
                AddLinksToAlertaDto(alerta);
            }
            return Ok(list);
        }

        /// <summary>
        /// Busca um alerta específico pelo seu identificador único (ID).
        /// </summary>
        /// <remarks>
        /// Este endpoint permite consultar os detalhes completos de um alerta específico.
        /// 
        /// Características:
        /// - Acesso público (não requer autenticação)
        /// - Retorna todos os dados do alerta
        /// - Não registra visualização do alerta
        /// - Não marca o alerta como descartado
        /// 
        /// Exemplo de uso:
        /// GET /api/alertas/123
        /// 
        /// Observações:
        /// - O ID do alerta deve ser um número válido
        /// - Este endpoint apenas consulta, não modifica o estado do alerta
        /// - Para marcar um alerta como visualizado ou descartado, use os endpoints específicos
        /// </remarks>
        /// <param name="id">Identificador único do alerta que deseja consultar</param>
        /// <response code="200">Retorna os dados completos do alerta encontrado</response>
        /// <response code="404">Retornado quando o ID informado não corresponde a nenhum alerta cadastrado</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AlertaDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<ActionResult<AlertaDto>> GetAlertaPorId([FromRoute] int id)
        {
            try
            {
                var alerta = await _service.GetAlertaByIdAsync(id);
                if (alerta == null)
                {
                    return NotFound(new ProblemDetails { Title = "Alerta não encontrado", Detail = $"Alerta com ID {id} não encontrado." });
                }
                AddLinksToAlertaDto(alerta);
                return Ok(alerta);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Recurso não encontrado", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Descarta um alerta específico para um usuário.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um usuário descarte (ocultar) um alerta específico.
        /// 
        /// Características:
        /// - Acesso público (não requer autenticação)
        /// - Realiza uma exclusão lógica (o alerta continua no sistema, mas fica oculto para o usuário)
        /// - O alerta descartado não aparecerá mais nas listagens do usuário
        /// 
        /// Exemplo de uso:
        /// DELETE /api/alertas/123?idUsuario=456
        /// 
        /// Observações:
        /// - O ID do alerta e do usuário são obrigatórios
        /// - O ID do usuário deve ser um número positivo
        /// - Esta operação é específica para um usuário (outros usuários ainda verão o alerta)
        /// </remarks>
        /// <param name="id">Identificador único do alerta que será descartado</param>
        /// <param name="idUsuario">Identificador do usuário que está descartando o alerta</param>
        /// <response code="204">Operação realizada com sucesso (alerta descartado ou já estava descartado)</response>
        /// <response code="400">ID do usuário inválido ou não fornecido</response>
        /// <response code="404">Alerta ou usuário não encontrado no sistema</response>
        [HttpDelete("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> DescartarAlertaPorUsuario([FromRoute] int id, [FromQuery] int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new ProblemDetails { Title = "ID de Usuário inválido", Detail = "O parâmetro idUsuario é obrigatório e deve ser um número positivo." });
            }
            try
            {
                await _service.DescartarAlertaParaUsuarioAsync(id, idUsuario);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Recurso não encontrado", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Remove permanentemente todos os alertas do sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite a exclusão completa de todos os alertas do sistema.
        /// 
        /// Características:
        /// - Requer autenticação JWT
        /// - Acesso restrito apenas para usuários com perfil de Administrador
        /// - Operação irreversível (não é possível recuperar os alertas após a exclusão)
        /// 
        /// Exemplo de uso:
        /// DELETE /api/alertas/todos
        /// 
        /// Observações:
        /// - É altamente recomendável fazer backup dos dados antes de executar esta operação
        /// - A operação retorna o número total de alertas que foram removidos
        /// - Esta ação afeta todos os usuários do sistema
        /// </remarks>
        /// <response code="200">Operação concluída com sucesso. Retorna o número de alertas removidos.</response>
        /// <response code="401">Usuário não autenticado. É necessário fornecer um token JWT válido.</response>
        /// <response code="403">Usuário autenticado não possui permissão de Administrador.</response>
        [HttpDelete("todos")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteTodosAlertas()
        {
            var count = await _service.DeleteTodosAlertasAsync();
            return Ok(new { message = $"{count} alertas foram deletados com sucesso." });
        }

        private void AddLinksToAlertaDto(AlertaDto alerta, int? idUsuario = null)
        {
            alerta.Links.Add(new ResourceLink(Url.Link(nameof(GetAlertaPorId), new { id = alerta.Id })!, "self", "GET"));
            
            if (idUsuario.HasValue)
            {
                 alerta.Links.Add(new ResourceLink(Url.Link(nameof(DescartarAlertaPorUsuario), new { id = alerta.Id, idUsuario = idUsuario.Value })!, "descartar-alerta-usuario", "DELETE"));
            }
        }
    }
} 