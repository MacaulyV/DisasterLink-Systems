using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DisasterLink_API.DTOs; // Assumindo que AbrigoTemporarioDto e AbrigoTemporarioCreateDto estarão aqui
using DisasterLink_API.Interfaces.Services;
using DisasterLink_API.SwaggerExamples; // Descomentado ou adicionado
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using DisasterLink_API.DTOs.Common; // Adicionado para ResourceLink

namespace DisasterLink_API.Controllers
{
    [ApiController]
    [Route("api/AbrigosTemporarios")]
    public class AbrigosTemporariosController : ControllerBase
    {
        private readonly IAbrigoTemporarioService _service;

        public AbrigosTemporariosController(IAbrigoTemporarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna uma lista completa de todos os abrigos temporários cadastrados no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar todos os abrigos temporários disponíveis.
        /// 
        /// Características:
        /// - Acesso público (não requer autenticação)
        /// - Retorna lista completa de abrigos
        /// - Inclui informações detalhadas de cada abrigo
        /// 
        /// Exemplo de uso:
        /// GET /api/AbrigosTemporarios
        /// 
        /// Observações:
        /// - A lista é retornada em ordem alfabética por nome do abrigo
        /// - Cada abrigo inclui links HATEOAS para ações relacionadas
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AbrigoTemporarioDto>), 200)]
        public async Task<ActionResult<IEnumerable<AbrigoTemporarioDto>>> GetMapa()
        {
            var list = await _service.GetAllAsync();
            foreach (var abrigo in list)
            {
                AddLinksToAbrigoDto(abrigo);
            }
            return Ok(list);
        }

        /// <summary>
        /// Retorna uma lista de abrigos temporários localizados em uma cidade específica.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite filtrar abrigos temporários por cidade/município.
        /// 
        /// Características:
        /// - Acesso público (não requer autenticação)
        /// - Retorna apenas abrigos da cidade especificada
        /// - Inclui informações detalhadas de cada abrigo
        /// 
        /// Exemplo de uso:
        /// GET /api/AbrigosTemporarios/cidade/municipio?nomeCidade=São Paulo
        /// 
        /// Observações:
        /// - A busca é case-insensitive (não diferencia maiúsculas de minúsculas)
        /// - Retorna lista vazia se nenhum abrigo for encontrado na cidade
        /// - Cada abrigo inclui links HATEOAS para ações relacionadas
        /// </remarks>
        /// <param name="nomeCidade">Nome da cidade ou município onde deseja buscar abrigos temporários</param>
        [HttpGet("cidade/municipio")]
        [ProducesResponseType(typeof(IEnumerable<AbrigoTemporarioDto>), 200)]
        public async Task<ActionResult<IEnumerable<AbrigoTemporarioDto>>> GetByCity([FromQuery] string nomeCidade)
        {
            var list = await _service.GetByCityAsync(nomeCidade);
            foreach (var abrigo in list)
            {
                AddLinksToAbrigoDto(abrigo);
            }
            return Ok(list);
        }

        /// <summary>
        /// Cadastra um novo abrigo temporário no sistema.
        /// </summary>
        /// <param name="dto">Objeto contendo os dados necessários para cadastro do abrigo temporário</param>
        /// <remarks>
        /// Este endpoint realiza duas operações principais:
        /// 1. Cadastro do abrigo temporário com todas as informações fornecidas
        /// 2. Geração automática de um alerta público contendo os dados do abrigo
        /// 
        /// Características importantes:
        /// - Requer autenticação JWT (usuário deve estar logado)
        /// - O alerta gerado é armazenado no banco de dados
        /// - O alerta será exibido no aplicativo mobile para todos os usuários
        /// 
        /// Exemplo de uso:
        /// POST /api/AbrigosTemporarios
        /// 
        /// Observações:
        /// - Todos os campos obrigatórios devem ser preenchidos
        /// - O alerta é gerado automaticamente após o cadastro bem-sucedido
        /// - O sistema valida os dados antes de processar o cadastro
        /// </remarks>
        [HttpPost(Name = "CreateAbrigoTemporario")]
        [Authorize]
        [SwaggerRequestExample(typeof(AbrigoTemporarioCreateDto), typeof(AbrigoTemporarioCreateDtoExample))]
        [ProducesResponseType(typeof(AbrigoTemporarioDto), 201)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AbrigoTemporarioDto>> Create([FromBody] AbrigoTemporarioCreateDto dto)
        {
            // TODO: A lógica de userId precisa ser revisada para o contexto de AbrigosTemporarios.
            // Por ora, vou manter a estrutura, mas isso pode precisar ser removido ou alterado
            // dependendo se um abrigo está ou não associado a um usuário criador específico no novo modelo.
            // Se não estiver, o parâmetro userId no service.CreateAsync também deverá ser removido.
            int userId = 1; // Placeholder - Revisar esta lógica!
            var created = await _service.CreateAsync(dto, userId);
            AddLinksToAbrigoDto(created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Busca e retorna os detalhes de um abrigo temporário específico através do seu identificador único.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite localizar um abrigo temporário específico no sistema.
        /// 
        /// Características:
        /// - Retorna todos os dados do abrigo se encontrado
        /// - Retorna 404 (Not Found) se o abrigo não existir
        /// - Inclui links HATEOAS para navegação relacionada
        /// 
        /// Exemplo de uso:
        /// GET /api/AbrigosTemporarios/{id}
        /// 
        /// Observações:
        /// - O ID deve ser um número inteiro válido
        /// - A resposta inclui todos os detalhes do abrigo
        /// </remarks>
        /// <param name="id">ID do abrigo temporário</param>
        [HttpGet("{id}", Name = "GetAbrigoTemporarioById")]
        [ProducesResponseType(typeof(AbrigoTemporarioDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AbrigoTemporarioDto>> GetById(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            
            AddLinksToAbrigoDto(dto);
            return Ok(dto);
        }

        /// <summary>
        /// Atualiza os dados de um abrigo temporário existente no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite modificar as informações de um abrigo temporário específico.
        /// 
        /// Características:
        /// - Requer autenticação JWT para acesso
        /// - Atualiza apenas os campos fornecidos no DTO
        /// - Retorna 204 (No Content) em caso de sucesso
        /// - Retorna 404 (Not Found) se o abrigo não existir
        /// - Retorna 400 (Bad Request) se os dados forem inválidos
        /// 
        /// Exemplo de uso:
        /// PATCH /api/AbrigosTemporarios/{id}
        /// 
        /// Observações:
        /// - O ID deve ser um número inteiro válido
        /// - Apenas os campos que deseja atualizar precisam ser enviados
        /// - Campos não enviados manterão seus valores originais
        /// </remarks>
        /// <param name="id">Identificador único do abrigo temporário a ser atualizado</param>
        /// <param name="dto">Objeto contendo os dados atualizados do abrigo temporário</param>
        [HttpPatch("{id}", Name = "UpdateAbrigoTemporario")]
        [Authorize]
        [SwaggerRequestExample(typeof(AbrigoTemporarioCreateDto), typeof(AbrigoTemporarioPatchDtoExample))]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] AbrigoTemporarioCreateDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Remove um abrigo temporário do sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite excluir permanentemente um abrigo temporário específico.
        /// 
        /// Características:
        /// - Requer autenticação JWT para acesso
        /// - Remove permanentemente o abrigo do sistema
        /// - Retorna 204 (No Content) em caso de sucesso
        /// - Retorna 404 (Not Found) se o abrigo não existir
        /// 
        /// Exemplo de uso:
        /// DELETE /api/AbrigosTemporarios/{id}
        /// 
        /// Observações:
        /// - O ID deve ser um número inteiro válido
        /// - A operação não pode ser desfeita
        /// - Todos os dados relacionados ao abrigo serão removidos
        /// </remarks>
        /// <param name="id">Identificador único do abrigo temporário a ser removido</param>
        [HttpDelete("{id}", Name = "DeleteAbrigoTemporario")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private void AddLinksToAbrigoDto(AbrigoTemporarioDto abrigo)
        {
            if (abrigo == null) return;

            abrigo.Links.Clear(); // Limpa links existentes para evitar duplicação se chamado múltiplas vezes

            // Link para o próprio recurso
            var selfLink = Url.Link("GetAbrigoTemporarioById", new { id = abrigo.Id });
            if (selfLink != null) 
                abrigo.Links.Add(new ResourceLink(selfLink, "self", "GET"));

            // Link para atualizar (PATCH)
            var updateLink = Url.Link("UpdateAbrigoTemporario", new { id = abrigo.Id });
            if (updateLink != null)
                abrigo.Links.Add(new ResourceLink(updateLink, "update", "PATCH"));

            // Link para deletar (DELETE)
            var deleteLink = Url.Link("DeleteAbrigoTemporario", new { id = abrigo.Id });
            if (deleteLink != null)
                abrigo.Links.Add(new ResourceLink(deleteLink, "delete", "DELETE"));

            // Link para listar todos os abrigos (coleção)
            var listAllLink = Url.Action(nameof(GetMapa), null, null, Request.Scheme);
            if (listAllLink != null)
                abrigo.Links.Add(new ResourceLink(listAllLink, "list-all", "GET"));
        }
    }
} 