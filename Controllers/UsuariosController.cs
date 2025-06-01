using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DisasterLink_API.DTOs;
using DisasterLink_API.Interfaces.Services;
using DisasterLink_API.SwaggerExamples;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using DisasterLink_API.DTOs.Auth;
using DisasterLink_API.DTOs.Common;

namespace DisasterLink_API.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuariosController(IUsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cadastra um novo usuário comum (cidadão) na plataforma.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite o registro de novos usuários cidadãos no sistema.
        /// 
        /// Informações importantes:
        /// - O endereço completo é opcional
        /// - O município pode ser informado posteriormente
        /// - O email deve ser único no sistema
        /// - A senha deve ter no mínimo 6 caracteres
        /// 
        /// Campos obrigatórios:
        /// - Nome completo
        /// - Email válido
        /// - Senha
        /// - Telefone
        /// 
        /// Campos opcionais:
        /// - Município
        /// - Bairro
        /// - Logradouro
        /// - Número
        /// - Complemento
        /// </remarks>
        /// <param name="dto">Objeto contendo os dados do novo usuário</param>
        /// <response code="201">Usuário cadastrado com sucesso. Retorna os dados do usuário criado.</response>
        /// <response code="400">Erro na validação dos dados:
        /// - Email já cadastrado
        /// - Dados obrigatórios ausentes
        /// - Formato de email inválido
        /// - Senha muito curta</response>
        [HttpPost("cadastrar", Name = "CreateUsuario")]
        [ProducesResponseType(typeof(UsuarioDto), 201)]
        [SwaggerRequestExample(typeof(UsuarioCreateDto), typeof(UsuarioCreateDtoExample))]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<UsuarioDto>> Create([FromBody] UsuarioCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                AddLinksToUsuarioDto(created);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { errors = new { email = ex.Message } });
            }
        }

        /// <summary>
        /// Realiza o login de um usuário comum (cidadão) na plataforma.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que usuários comuns (cidadãos) façam login no sistema.
        /// 
        /// Requisitos para login:
        /// - Email válido (deve conter "@")
        /// - Senha com no mínimo 6 caracteres
        /// 
        /// Observações importantes:
        /// - Este login é específico para usuários comuns (cidadãos)
        /// - Não gera token JWT pois as operações de usuário comum não requerem autenticação JWT
        /// - Ideal para acesso básico às funcionalidades da plataforma
        /// 
        /// O que acontece após o login:
        /// - Verifica se as credenciais são válidas
        /// - Retorna os dados básicos do usuário
        /// - Indica se o login foi bem-sucedido
        /// </remarks>
        /// <param name="dto">Dados de login (email e senha)</param>
        /// <response code="200">Login realizado com sucesso. Retorna os dados do usuário e confirmação de sucesso.</response> 
        /// <response code="400">Erro na validação dos dados:
        /// - Email inválido
        /// - Senha muito curta
        /// - Formato de email incorreto</response>
        /// <response code="401">Falha na autenticação:
        /// - Email não encontrado
        /// - Senha incorreta</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUsuarioResponseDto), 200)]
        [SwaggerRequestExample(typeof(LoginDto), typeof(LoginDtoExample))]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginUsuarioResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _service.LoginAsync(dto);
                if (!response.Sucesso)
                {
                    return Unauthorized(new { errors = new { login = response.Mensagem } });
                }
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { errors = new { login = ex.Message } });
            }
        }

        /// <summary>
        /// Consulta o perfil de um usuário específico através do seu ID.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar os dados públicos de qualquer usuário cadastrado na plataforma.
        /// 
        /// Características:
        /// - Acesso público (não requer autenticação)
        /// - Retorna apenas informações públicas do perfil
        /// - Dados sensíveis são ocultados automaticamente
        /// 
        /// Exemplo de uso:
        /// - Para ver o perfil do usuário com ID 123, faça uma requisição GET para /api/usuarios/123
        /// 
        /// Observações:
        /// - Este endpoint é útil para visualizar perfis de outros usuários
        /// - Ideal para verificar informações de pontos de coleta ou doadores
        /// </remarks>
        /// <param name="id">ID único do usuário que deseja consultar</param>
        /// <response code="200">Retorna os dados públicos do usuário encontrado</response>
        /// <response code="404">Retornado quando o ID informado não corresponde a nenhum usuário cadastrado</response>
        [HttpGet("{id}", Name = "GetUsuarioById")]
        [ProducesResponseType(typeof(UsuarioDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioDto>> GetById(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound(new { errors = new { usuario = "Não encontrado." } });
            AddLinksToUsuarioDto(dto);
            return Ok(dto);
        }

        /// <summary>
        /// Lista todos os usuários cadastrados no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar todos os usuários registrados na plataforma.
        /// 
        /// Características:
        /// - Acesso exclusivo para administradores
        /// - Requer autenticação via token JWT
        /// - Retorna lista completa de usuários
        /// 
        /// Exemplo de uso:
        /// - Faça uma requisição GET para /api/usuarios com token JWT de administrador
        /// 
        /// Observações:
        /// - Apenas usuários com perfil de administrador podem acessar
        /// - Token JWT deve ser enviado no header da requisição
        /// </remarks>
        /// <response code="200">Retorna a lista completa de usuários cadastrados</response>
        /// <response code="401">Retornado quando o token JWT é inválido ou não está presente</response>
        [HttpGet(Name = "GetAllUsuarios")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<UsuarioDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<UsuarioDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            foreach (var usuario in list)
            {
                AddLinksToUsuarioDto(usuario);
            }
            return Ok(list);
        }

        /// <summary>
        /// Atualiza os dados cadastrais de um usuário existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar informações de um usuário já cadastrado no sistema.
        /// 
        /// Características:
        /// - Atualização parcial dos dados (PATCH)
        /// - Requer autenticação via token JWT
        /// - Apenas o próprio usuário ou administradores podem atualizar
        /// 
        /// Dados que podem ser atualizados:
        /// - Nome completo
        /// - E-mail
        /// - Telefone
        /// - Endereço
        /// 
        /// Exemplo de uso:
        /// - Faça uma requisição PATCH para /api/usuarios/{id} com os dados a serem atualizados
        /// 
        /// Observações:
        /// - O ID do usuário não pode ser alterado
        /// - A senha deve ser alterada através do endpoint específico de alteração de senha
        /// - Token JWT deve ser enviado no header da requisição
        /// </remarks>
        [HttpPatch("{id}", Name = "UpdateUsuario")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [SwaggerRequestExample(typeof(UsuarioUpdateDto), typeof(UsuarioUpdateDtoExample))]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { errors = new { usuario = "Não encontrado." } });
            }
        }

            /// <summary>
            /// Remove permanentemente um usuário do sistema através do seu ID. 
            /// </summary>
            /// <remarks>
            /// Este endpoint permite a exclusão definitiva de um usuário do sistema.
            /// 
            /// Características:
            /// - Exclusão permanente (não é possível recuperar os dados)
            /// - Requer autenticação via token JWT
            /// - Apenas o próprio usuário ou administradores podem excluir
            /// 
            /// O que acontece:
            /// - Todos os dados do usuário são removidos do banco de dados
            /// - Histórico de atividades é mantido por questões de auditoria
            /// - O usuário não poderá mais acessar o sistema com este ID
            /// 
            /// Exemplo de uso:
            /// - Faça uma requisição DELETE para /api/usuarios/{id}
            /// 
            /// Observações:
            /// - A exclusão é irreversível
            /// - Recomenda-se fazer backup dos dados importantes antes
            /// - Token JWT deve ser enviado no header da requisição
            /// </remarks>
        /// <param name="id">ID do usuário a ser excluído.</param>
        [HttpDelete("{id}", Name = "DeleteUsuario")]
        [ProducesResponseType(204)] // NoContent
        [ProducesResponseType(404)] // NotFound
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { errors = new { usuario = "Usuário não encontrado para exclusão." } });
            }
            // Adicionar catch para outras exceções se necessário, ex: problemas de concorrência ou falhas no DB.
        }

        /// <summary>
        /// Endpoint para redefinição de senha do usuário.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um usuário redefina sua senha quando esqueceu a senha atual.
        /// 
        /// Como funciona:
        /// 1. O usuário fornece seu e-mail cadastrado
        /// 2. Informa a nova senha desejada
        /// 3. Confirma a nova senha
        /// 
        /// Validações realizadas:
        /// - E-mail deve estar cadastrado no sistema
        /// - Nova senha deve atender aos requisitos de segurança
        /// - Confirmação de senha deve ser idêntica à nova senha
        /// </remarks>
        /// <param name="dto">Objeto contendo:
        /// - Email: e-mail cadastrado do usuário
        /// - NovaSenha: nova senha desejada
        /// - ConfirmacaoSenha: confirmação da nova senha</param>
        /// <returns>
        /// - 200: Senha redefinida com sucesso
        /// - 400: Dados inválidos ou senhas não conferem
        /// - 404: E-mail não encontrado no sistema
        /// </returns>
        [HttpPost("esqueceu-senha")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [SwaggerRequestExample(typeof(UsuarioForgotPasswordDto), typeof(UsuarioForgotPasswordDtoExample))]
        public async Task<IActionResult> ForgotPassword([FromBody] UsuarioForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _service.ForgotPasswordAsync(dto);
                return Ok(new { message = "Senha redefinida com sucesso." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Nenhum usuário encontrado com este e-mail." });
            }
            catch (ArgumentException ex) // Para senhas diferentes ou outras validações do serviço
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Busca usuários por localização geográfica. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite encontrar usuários cadastrados em uma determinada região:
        /// 
        /// Como funciona:
        /// 1. Você pode buscar por:
        ///    - Apenas cidade/município
        ///    - Apenas bairro
        ///    - Cidade/município E bairro juntos
        /// 
        /// 2. Exemplos de uso:
        ///    - Buscar todos os usuários de São Paulo
        ///    - Buscar usuários do bairro Jardins
        ///    - Buscar usuários do bairro Jardins em São Paulo
        /// 
        /// 3. Regras importantes:
        ///    - É necessário fornecer pelo menos um critério de busca
        ///    - Apenas administradores podem usar este recurso
        ///    - É necessário estar autenticado com token JWT válido
        /// 
        /// 4. O que você recebe:
        ///    - Lista de usuários que correspondem aos critérios
        ///    - Dados básicos de cada usuário encontrado
        /// </remarks>
        /// <param name="cidadeMunicipio">Nome da cidade ou município (opcional)</param>
        /// <param name="bairro">Nome do bairro (opcional)</param>
        /// <returns>
        /// - 200: Lista de usuários encontrados
        /// - 400: Nenhum critério de busca informado
        /// - 401: Não autorizado (token inválido)
        /// - 404: Nenhum usuário encontrado
        /// </returns>
        [HttpPost("auth/google-login")]
        [ProducesResponseType(typeof(LoginUsuarioResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        // Adicionar SwaggerRequestExample se desejar
        public async Task<ActionResult<LoginUsuarioResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            if (!ModelState.IsValid) // Embora o DTO só tenha um campo required, é uma boa prática
            {
                return BadRequest(ModelState);
            }

            var response = await _service.LoginWithGoogleAsync(dto);

            if (!response.Sucesso)
            {
                return BadRequest(new { error = response.Mensagem });
            }

            return Ok(response);
        }

        /// <summary>
        /// Realiza login de usuário através do Google OAuth. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que usuários façam login no sistema usando suas credenciais do Google:
        /// 
        /// 1. Como funciona:
        ///    - Usuário fornece o token de autenticação do Google
        ///    - Sistema valida o token com o Google
        ///    - Se válido, cria/atualiza o usuário no sistema
        ///    - Retorna token JWT para autenticação
        /// 
        /// 2. Fluxo de autenticação:
        ///    - Cliente obtém token do Google
        ///    - Envia token para este endpoint
        ///    - Recebe token JWT para uso no sistema
        /// 
        /// 3. Importante:
        ///    - Token do Google deve ser válido
        ///    - Usuário é criado automaticamente se não existir
        ///    - Dados básicos são sincronizados do Google
        /// </remarks>
        /// <param name="dto">Token de autenticação do Google</param>
        /// <returns>
        /// - 200: Login realizado com sucesso
        /// - 400: Token inválido ou erro na autenticação
        /// </returns>
        [HttpGet("localidade", Name = "GetUsuariosByLocalidade")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<UsuarioDto>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<UsuarioDto>>> GetByLocalidade([FromQuery] string? cidadeMunicipio, [FromQuery] string? bairro)
        {
            if (string.IsNullOrWhiteSpace(cidadeMunicipio) && string.IsNullOrWhiteSpace(bairro))
            {
                return BadRequest(new { error = "Ao menos um critério de busca (cidadeMunicipio ou bairro) deve ser fornecido." });
            }

            var list = await _service.GetByLocalidadeAsync(cidadeMunicipio, bairro);
            if (list == null || !list.Any())
            {
                 return NotFound(new { message = "Nenhum usuário encontrado para os critérios fornecidos." });
            }
            foreach (var usuario in list)
            {
                AddLinksToUsuarioDto(usuario);
            }
            return Ok(list);
        }

        private void AddLinksToUsuarioDto(UsuarioDto usuario)
        {
            if (usuario == null) return;

            usuario.Links.Clear();

            var selfLink = Url.Link("GetUsuarioById", new { id = usuario.Id });
            if (selfLink != null) usuario.Links.Add(new ResourceLink(selfLink, "self", "GET"));

            // Assumindo que usuários podem ser atualizados e deletados (verificar permissões se necessário)
            var updateLink = Url.Link("UpdateUsuario", new { id = usuario.Id });
            if (updateLink != null) usuario.Links.Add(new ResourceLink(updateLink, "update", "PATCH"));

            var deleteLink = Url.Link("DeleteUsuario", new { id = usuario.Id });
            if (deleteLink != null) usuario.Links.Add(new ResourceLink(deleteLink, "delete", "DELETE"));
            
            // Link para a coleção de usuários (se aplicável, ex: para admins)
             var listAllLink = Url.Link("GetAllUsuarios", null);
            if (listAllLink != null) usuario.Links.Add(new ResourceLink(listAllLink, "list-all", "GET"));
        }
    }
} 