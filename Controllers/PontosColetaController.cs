using DisasterLink_API.DTOs.Create;
using DisasterLink_API.DTOs.Response;
using DisasterLink_API.DTOs.Update;
using DisasterLink_API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Endpoints para gerenciar Pontos de Coleta de Doações")]
    public class PontosColetaController : ControllerBase
    {
        private readonly IPontoDeColetaDeDoacoesService _pontoColetaService;

        public PontosColetaController(IPontoDeColetaDeDoacoesService pontoColetaService)
        {
            _pontoColetaService = pontoColetaService;
        }

        /// <summary>
        /// Lista todos os pontos de coleta ativos no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite buscar pontos de coleta de doações com filtros opcionais:
        /// 
        /// 1. Filtro por Cidade:
        ///    - Você pode especificar uma cidade para ver apenas os pontos de coleta daquela região
        ///    - Exemplo: São Paulo, Rio de Janeiro, etc.
        /// 
        /// 2. Filtro por Tipo:
        ///    - Você pode filtrar por tipo específico de doação
        ///    - Exemplos: "Arrecadação de alimentos", "Coleta de roupas", "Doação de medicamentos"
        /// 
        /// Observações:
        /// - Os filtros são opcionais - se não forem informados, retorna todos os pontos ativos
        /// - Apenas pontos de coleta ativos são retornados
        /// - Os resultados são ordenados por relevância
        /// </remarks>
        /// <param name="cidade">Nome da cidade para filtrar os pontos de coleta (opcional)</param>
        /// <param name="tipo">Tipo específico de doação para filtrar os pontos (opcional)</param>
        /// <response code="200">Retorna a lista de pontos de coleta encontrados com sucesso</response>
        [HttpGet(Name = "GetAllPontosColeta")]
        [SwaggerOperation(Summary = "Lista pontos de coleta ativos", Description = "Retorna uma lista de pontos de coleta de doações ativos, com filtros opcionais.")]
        [ProducesResponseType(typeof(IEnumerable<PontoDeColetaDeDoacoesDto>), 200)]
        public async Task<ActionResult<IEnumerable<PontoDeColetaDeDoacoesDto>>> GetAllPontosColeta([FromQuery] string? cidade, [FromQuery] string? tipo)
        {
            var pontosColeta = await _pontoColetaService.GetAllPontosDeColetaAsync(cidade, tipo);
            foreach (var ponto in pontosColeta)
            {
                AddLinksToPontoColetaDto(ponto);
            }
            return Ok(pontosColeta);
        }

        /// <summary>
        /// Lista todos os pontos de coleta ativos no sistema, sem aplicar nenhum filtro.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista completa de todos os pontos de coleta ativos, 
        /// independentemente de:
        /// - Localização (cidade, estado, região)
        /// - Tipo de doação aceita
        /// - Outros critérios de filtragem
        /// 
        /// Útil para:
        /// - Visualização geral de todos os pontos disponíveis
        /// - Análise completa da rede de pontos de coleta
        /// - Situações onde não se deseja aplicar filtros específicos
        /// </remarks>
        /// <response code="200">Retorna a lista completa de todos os pontos de coleta ativos no sistema.</response>
        [HttpGet("todos", Name = "GetAllPontosColetaSemFiltro")]
        [SwaggerOperation(Summary = "Lista todos os pontos de coleta (sem filtro)", Description = "Retorna uma lista de todos os pontos de coleta de doações ativos.")]
        [ProducesResponseType(typeof(IEnumerable<PontoDeColetaDeDoacoesDto>), 200)]
        public async Task<ActionResult<IEnumerable<PontoDeColetaDeDoacoesDto>>> GetAllPontosColetaSemFiltro()
        {
            var pontosColeta = await _pontoColetaService.GetAllPontosDeColetaAsync(null, null);
            foreach (var ponto in pontosColeta)
            {
                AddLinksToPontoColetaDto(ponto);
            }
            return Ok(pontosColeta);
        }

        /// <summary>
        /// Busca um ponto de coleta específico pelo ID.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite localizar um ponto de coleta específico através do seu ID único.
        /// 
        /// Como funciona:
        /// 1. Forneça o ID do ponto de coleta desejado
        /// 2. O sistema retorna todos os detalhes do ponto encontrado
        /// 
        /// O que você receberá:
        /// - Informações completas do ponto de coleta
        /// - Endereço e localização
        /// - Tipos de doações aceitas
        /// - Horário de funcionamento
        /// - Contatos e outras informações relevantes
        /// 
        /// Observações:
        /// - Se o ID não existir, receberá um erro 404
        /// - Este endpoint é público e não requer autenticação
        /// </remarks>
        [HttpGet("{id}", Name = "GetPontoColetaById")]
        [SwaggerOperation(Summary = "Busca ponto de coleta por ID", Description = "Retorna um ponto de coleta de doações específico.")]
        [ProducesResponseType(typeof(PontoDeColetaDeDoacoesDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PontoDeColetaDeDoacoesDto>> GetPontoColetaById(int id)
        {
            var ponto = await _pontoColetaService.GetPontoDeColetaByIdAsync(id); 
            if (ponto == null)
            {
                return NotFound(new { message = $"Ponto de coleta com ID {id} não encontrado." });
            }
            AddLinksToPontoColetaDto(ponto);
            return Ok(ponto);
        }

        /// <summary>
        /// Cria um novo ponto de coleta destinado ao usuario. 
        /// </summary>
        /// <remarks>
        /// Instruções para criar um novo ponto de coleta:
        /// 
        /// 1. Campos Obrigatórios:
        ///    - Nome do ponto de coleta
        ///    - Endereço completo
        ///    - Tipo de doação aceita
        ///    - Data de início (formato: yyyy-MM-dd)
        ///    - Horário de funcionamento
        /// 
        /// 2. Campos Opcionais:
        ///    - imagemUrl: URL da imagem do ponto de coleta
        ///    - Descrição adicional
        ///    - Contatos para mais informações
        /// 
        /// 3. Comportamento do Sistema:
        ///    - Ao criar um ponto de coleta, um alerta público é gerado automaticamente
        ///    - O alerta permanecerá ativo até ser manualmente removido
        ///    - Cada usuário terá sua própria visualização do alerta
        /// 
        /// 4. Observações:
        ///    - Apenas administradores podem criar pontos de coleta
        ///    - É necessário estar autenticado com token JWT válido
        ///    - Todos os campos são validados antes da criação
        /// </remarks>
        /// <param name="pontoColetaDto">Dados para criação do ponto de coleta. 
        /// </param>
        /// <response code="201">Ponto de coleta criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Não autorizado.</response>
        /// <response code="403">Acesso negado (não é Admin).</response>
        [HttpPost("criar", Name = "CreatePontoColeta")]
        [Authorize(Roles = "Admin")] // 🔐 JWT Admin
        [SwaggerOperation(Summary = "Cria um novo ponto de coleta 🔐", Description = "Cria um novo ponto de coleta de doações. Requer autenticação JWT e perfil de Admin.")]
        [ProducesResponseType(typeof(PontoDeColetaDeDoacoesDto), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PontoDeColetaDeDoacoesDto>> CreatePontoColeta([FromBody] PontoDeColetaDeDoacoesCreateDto pontoColetaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var novoPontoColeta = await _pontoColetaService.CreatePontoDeColetaAsync(pontoColetaDto);
                AddLinksToPontoColetaDto(novoPontoColeta);
                return CreatedAtAction(nameof(GetPontoColetaById), new { id = novoPontoColeta.Id }, novoPontoColeta);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
        }

        /// <summary>
        /// Atualiza parcialmente um ponto de coleta existente. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar informações específicas de um ponto de coleta.
        /// 
        /// Como funciona:
        /// 1. Você só precisa enviar os campos que deseja atualizar
        /// 2. Os campos não enviados permanecerão com seus valores atuais
        /// 3. Para remover uma imagem, envie "imagemUrl": null
        /// 4. Campos opcionais podem ser omitidos
        /// 
        /// Exemplo de uso:
        /// Para atualizar apenas a descrição e o estoque, envie:
        /// {
        ///   "descricao": "Nova descrição da coleta de cestas básicas para famílias afetadas e desabrigadas.",
        ///   "estoque": "500 unidades"
        /// }
        /// 
        /// Observações importantes:
        /// - Apenas administradores podem fazer atualizações
        /// - É necessário estar autenticado com token JWT válido
        /// - A data de início não pode ser alterada após a criação
        /// </remarks>
        /// <param name="id">ID do ponto de coleta que será atualizado</param>
        /// <param name="pontoDeColetaUpdateDto">Dados que serão atualizados</param>
        /// <response code="200">Atualização realizada com sucesso</response>
        /// <response code="400">Dados inválidos ou tentativa de alterar dataInicio</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário não tem permissão de administrador</response>
        /// <response code="404">Ponto de coleta não encontrado</response>
        [HttpPatch("{id}", Name = "UpdatePontoColeta")]
        [Authorize(Roles = "Admin")] // 🔐 JWT Admin
        [SwaggerOperation(Summary = "Atualiza um ponto de coleta (PATCH) 🔐", Description = "Atualiza parcialmente um ponto de coleta existente. Requer autenticação JWT e perfil de Admin.")]
        [ProducesResponseType(typeof(PontoDeColetaDeDoacoesDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PontoDeColetaDeDoacoesDto>> UpdatePontoColeta(int id, [FromBody] PontoDeColetaDeDoacoesUpdateDto pontoDeColetaUpdateDto)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pontoAtualizado = await _pontoColetaService.UpdatePontoDeColetaAsync(id, pontoDeColetaUpdateDto);
                if (pontoAtualizado == null)
                {
                    return NotFound(new { message = $"Ponto de coleta com ID {id} não encontrado." });
                }
                AddLinksToPontoColetaDto(pontoAtualizado);
                return Ok(pontoAtualizado);
            }
            catch (ArgumentException ex) 
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
        }

        /// <summary>
        /// Registra a participação de um usuário em um ponto de coleta de doações.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um usuário se registre para participar de um ponto de coleta de doações.
        /// 
        /// Informações Geradas Automaticamente:
        /// - ID da participação: número de 4 dígitos
        /// - Data e hora: registrada no horário de Brasília
        /// 
        /// Parâmetros da URL:
        /// - pontoColetaId: ID do ponto de coleta onde deseja participar
        /// - idUsuario: ID do usuário que está se registrando
        /// 
        /// Dados Obrigatórios (no corpo da requisição):
        /// - formaDeAjuda: como pretende ajudar
        /// - telefone: número para contato
        /// 
        /// Dados Opcionais (no corpo da requisição):
        /// - mensagem: observações adicionais
        /// - contato: nome de contato alternativo
        /// </remarks>
        /// <param name="pontoColetaId">ID do ponto de coleta (4 dígitos) onde registrar a participação (da URL).</param>
        /// <param name="idUsuario">ID do usuário participante (4 dígitos) (da query string).</param>
        /// <param name="dto">Dados da participação: forma de ajuda, mensagem (opcional), contato (opcional) e telefone.</param>
        /// <response code="201">Participação registrada com sucesso. Retorna os dados da participação, incluindo ID e data/hora gerados.</response>
        /// <response code="400">Erro de validação nos dados enviados (ex: campos obrigatórios ausentes no corpo, IDs inválidos).</response>
        /// <response code="404">Ponto de coleta ou usuário não encontrados.</response>
        [HttpPost("{pontoColetaId}/participar")] // Rota atualizada
        [SwaggerOperation(Summary = "Registra participação de usuário em ponto de coleta", Description = "Permite que um usuário registre sua intenção de participar em um ponto de coleta. O ID da participação e a data/hora são gerados automaticamente.")]
        [ProducesResponseType(typeof(ParticipacaoPontoColetaDto), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ParticipacaoPontoColetaDto>> AddParticipacao(
            [FromRoute] int pontoColetaId, 
            [FromQuery] int idUsuario, 
            [FromBody] ParticipacaoPontoColetaCreateDto dto)
        {
            if (!ModelState.IsValid) // Validação do DTO (corpo da requisição)
            {
                return BadRequest(ModelState);
            }

            // Validações adicionais para IDs podem ser feitas aqui se necessário, 
            // embora o serviço já vá verificar a existência.
            if (pontoColetaId <= 0)
            {
                ModelState.AddModelError(nameof(pontoColetaId), "O ID do ponto de coleta deve ser um número positivo.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            if (idUsuario <= 0)
            {
                ModelState.AddModelError(nameof(idUsuario), "O ID do usuário deve ser um número positivo.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            try
            {
                var novaParticipacao = await _pontoColetaService.AddParticipacaoAsync(pontoColetaId, idUsuario, dto);
                
                if (novaParticipacao == null)
                {
                    // O serviço retorna null se o Ponto de Coleta ou Usuário não forem encontrados.
                    // Ou se alguma outra lógica de negócio impedir a criação (embora aqui seja mais para NotFound).
                    return NotFound(new { message = $"Ponto de coleta com ID {pontoColetaId} ou Usuário com ID {idUsuario} não encontrado, ou falha ao processar participação." });
                }
                
                // Retorna 201 Created com a localização do novo recurso (opcional, mas boa prática)
                // Para isso, precisaríamos de um endpoint GetParticipacaoById, que não temos atualmente.
                // Por simplicidade, retornaremos 201 com o objeto criado.
                return StatusCode(201, novaParticipacao);
            }
            catch (KeyNotFoundException ex) // Captura exceções específicas se o serviço as lançar
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Para outros erros de validação que o serviço possa lançar
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
        }

        /// <summary>
        /// Lista os participantes de um ponto de coleta.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista completa de todos os usuários que estão participando
        /// de um determinado ponto de coleta. Para cada participante, são retornadas informações
        /// como:
        /// - ID da participação
        /// - ID do usuário participante
        /// - Nome do usuário
        /// - Data de início da participação
        /// - Status da participação
        /// - Outros detalhes relevantes
        /// 
        /// A lista pode estar vazia se não houver participantes registrados.
        /// </remarks>
        /// <param name="id">ID do ponto de coleta.</param>
        /// <response code="200">Retorna a lista de participantes.</response>
        /// <response code="404">Ponto de coleta não encontrado.</response>
        [HttpGet("{id}/participantes")]
        [SwaggerOperation(Summary = "Lista os participantes de um ponto de coleta", Description = "Retorna a lista de todas as participações registradas para um ponto de coleta específico.")]
        [ProducesResponseType(typeof(IEnumerable<ParticipacaoPontoColetaDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<ParticipacaoPontoColetaDto>>> GetParticipantes(int id)
        {
            var participantes = await _pontoColetaService.GetParticipantesAsync(id);
            // Se o serviço retornar uma lista vazia, isso pode significar que o ponto de coleta não foi encontrado
            // OU que não há participantes. Por simplicidade, retornamos a lista vazia em ambos os casos.
            // Alternativamente, o serviço poderia lançar uma exceção ou retornar null se o ponto não existir.
            return Ok(participantes);
        }

        /// <summary>
        /// Remove permanentemente um ponto de coleta do sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite a exclusão completa de um ponto de coleta. 
        /// 
        /// Importante:
        /// - Apenas administradores podem executar esta operação
        /// - A exclusão é permanente e não pode ser desfeita
        /// - Todos os dados relacionados ao ponto de coleta serão removidos
        /// 
        /// Requisitos:
        /// - Autenticação JWT válida
        /// - Perfil de administrador
        /// </remarks>
        /// <param name="id">Identificador único do ponto de coleta que será removido</param>
        /// <response code="204">Operação concluída com sucesso. O ponto de coleta foi removido.</response>
        /// <response code="401">Falha na autenticação. Token JWT inválido ou ausente.</response>
        /// <response code="403">Acesso negado. Usuário não possui permissão de administrador.</response>
        /// <response code="404">Ponto de coleta não encontrado no sistema.</response>
        [HttpDelete("{id}", Name = "DeletePontoColeta")]
        [Authorize(Roles = "Admin")] // 🔐 JWT Admin
        [SwaggerOperation(Summary = "Exclui um ponto de coleta 🔐", Description = "Exclui um ponto de coleta existente. Requer autenticação JWT e perfil de Admin.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePontoColeta(int id)
        {
            try
            {
                var success = await _pontoColetaService.DeletePontoDeColetaAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Ponto de coleta com ID {id} não encontrado." });
                }
                return NoContent();
            }
            catch (Exception ex) // Tratamento genérico para outras possíveis exceções
            {
                // Logar a exceção (não incluído aqui para brevidade)
                return StatusCode(500, new { message = "Ocorreu um erro interno ao tentar excluir o ponto de coleta.", details = ex.Message });
            }
        }

        private void AddLinksToPontoColetaDto(PontoDeColetaDeDoacoesDto ponto)
        {
            if (ponto == null) return;

            ponto.Links.Clear();

            var selfLink = Url.Link("GetPontoColetaById", new { id = ponto.Id });
            if (selfLink != null) ponto.Links.Add(new ResourceLink(selfLink, "self", "GET"));

            var updateLink = Url.Link("UpdatePontoColeta", new { id = ponto.Id });
            if (updateLink != null) ponto.Links.Add(new ResourceLink(updateLink, "update", "PATCH"));

            var deleteLink = Url.Link("DeletePontoColeta", new { id = ponto.Id });
            if (deleteLink != null) ponto.Links.Add(new ResourceLink(deleteLink, "delete", "DELETE"));
            
            // Link para registrar participação
            var participarLink = Url.Action(nameof(AddParticipacao), new { pontoColetaId = ponto.Id });
            if (participarLink != null) ponto.Links.Add(new ResourceLink(participarLink + "?idUsuario={idUsuario}", "participar", "POST"));

            // Link para listar participantes
            var participantesLink = Url.Action(nameof(GetParticipantes), new { id = ponto.Id });
            if (participantesLink != null) ponto.Links.Add(new ResourceLink(participantesLink, "participantes", "GET"));
        }
    }
} 