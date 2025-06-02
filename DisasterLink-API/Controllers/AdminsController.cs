using DisasterLink_API.DTOs.Admin;
using DisasterLink_API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.SwaggerExamples;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Cadastra um novo administrador no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite criar um novo usuário com permissões administrativas.
        /// 
        /// Características:
        /// - Acesso restrito apenas para administradores autenticados
        /// - Criação de conta administrativa completa
        /// - Validação automática de dados
        /// 
        /// Requisitos de senha:
        /// - Mínimo de 6 caracteres
        /// - Recomenda-se usar letras maiúsculas, minúsculas e números
        /// 
        /// Exemplo de uso:
        /// POST /api/admin/cadastrar
        /// 
        /// Observações:
        /// - Email deve ser único no sistema
        /// - Todos os campos obrigatórios devem ser preenchidos
        /// - A senha será criptografada automaticamente
        /// </remarks>
        /// <param name="adminCreateDto">Objeto contendo os dados do novo administrador</param>
        /// <response code="201">Retorna os dados do administrador criado com sucesso</response>
        /// <response code="400">Retornado quando:
        /// - Email já está cadastrado
        /// - Dados obrigatórios não foram preenchidos
        /// - Formato de dados inválido
        /// </response>
        [Authorize(Roles = "Admin")]
        [HttpPost("cadastrar")]  
        [ProducesResponseType(typeof(AdminDto), 201)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AdminDto>> Cadastrar([FromBody] AdminCreateDto adminCreateDto)
        {
            try
            {
                var admin = await _adminService.CreateAsync(adminCreateDto);
                return CreatedAtAction(nameof(GetAdminPorId), new { id = admin.Id }, admin);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { errors = new { general = ex.Message } });
            }
        }

        /// <summary>
        /// 🔑 Realiza a autenticação de um administrador no sistema e obtém o token JWT.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que administradores façam login no sistema.
        /// 
        /// Características:
        /// - Autenticação via email e senha
        /// - Geração de token JWT para acesso seguro
        /// - Validação automática das credenciais
        /// 
        /// Requisitos:
        /// - Email válido (deve conter "@")
        /// - Senha com mínimo de 6 caracteres
        /// 
        /// O que você receberá:
        /// - Token JWT para autenticação
        /// - Dados básicos do administrador
        /// - Informações de acesso
        /// 
        /// Exemplo de uso:
        /// POST /api/admin/login
        /// 
        /// Observações:
        /// - O token JWT deve ser usado em requisições subsequentes
        /// - O token expira após um período determinado
        /// - Mantenha suas credenciais em segurança
        /// </remarks>
        /// <param name="loginDto">Objeto contendo email e senha do administrador</param>
        /// <response code="200">Login realizado com sucesso. Retorna token JWT e dados do administrador</response>
        /// <response code="400">Retornado quando:
        /// - Email ou senha inválidos
        /// - Credenciais não correspondem a um administrador
        /// - Dados obrigatórios não foram preenchidos
        /// </response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AdminLoginResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AdminLoginResponseDto>> Login([FromBody] AdminLoginDto loginDto)
        {
            try
            {
                var response = await _adminService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { errors = new { login = ex.Message } });
            }
        }

        /// <summary>
        /// Busca um administrador específico pelo seu ID único.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite consultar os dados detalhados de um administrador específico do sistema.
        /// 
        /// Características:
        /// - Acesso restrito apenas para administradores autenticados
        /// - Requer token JWT válido no header da requisição
        /// - Retorna dados completos do administrador solicitado
        /// 
        /// Como usar:
        /// 1. Faça uma requisição GET para /api/admins/{id}
        /// 2. Inclua o token JWT no header Authorization
        /// 3. Substitua {id} pelo ID do administrador desejado
        /// 
        /// Observações:
        /// - Apenas administradores podem acessar este endpoint
        /// - O token JWT deve estar válido e não expirado
        /// - O ID informado deve corresponder a um administrador existente
        /// </remarks>
        /// <param name="id">ID único do administrador que deseja consultar</param>
        /// <response code="200">Retorna os dados completos do administrador encontrado</response>
        /// <response code="401">Retornado quando o token JWT é inválido ou não está presente</response>
        /// <response code="404">Retornado quando o ID informado não corresponde a nenhum administrador cadastrado</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AdminDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AdminDto>> GetAdminPorId(int id)
        {
            try
            {
                var admin = await _adminService.GetByIdAsync(id);
                return Ok(admin);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { errors = new { admin = ex.Message } });
            }
        }

        /// <summary>
        /// Lista todos os administradores cadastrados no sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite visualizar todos os administradores registrados no sistema.
        /// 
        /// Características:
        /// - Acesso restrito apenas para administradores autenticados
        /// - Requer token JWT válido no header da requisição
        /// - Retorna lista completa de administradores com seus dados
        /// 
        /// Como usar:
        /// 1. Faça uma requisição GET para /api/admins
        /// 2. Inclua o token JWT no header Authorization
        /// 
        /// Observações:
        /// - Apenas administradores podem acessar este endpoint
        /// - O token JWT deve estar válido e não expirado
        /// </remarks>
        /// <response code="200">Retorna a lista completa de administradores cadastrados</response>
        /// <response code="401">Retornado quando o token JWT é inválido ou não está presente</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<AdminDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<AdminDto>>> ListarAdmins()
        {
            var admins = await _adminService.GetAllAsync();
            return Ok(admins);
        }

        /// <summary>
        /// Atualiza os dados de um administrador no sistema. 
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar informações de um administrador existente.
        /// 
        /// O que pode ser atualizado:
        /// - Nome do administrador
        /// - Senha de acesso
        /// 
        /// Restrições importantes:
        /// - O email não pode ser alterado
        /// - Para alterar a senha, é necessário fornecer a senha atual
        /// - A nova senha deve ter no mínimo 6 caracteres
        /// 
        /// Quem pode usar:
        /// - O próprio administrador
        /// - Outros administradores do sistema
        /// 
        /// Como usar:
        /// 1. Faça uma requisição PATCH para /api/admins/{id}/editar
        /// 2. Inclua o token JWT no header Authorization
        /// 3. Envie os dados a serem atualizados no corpo da requisição
        /// 
        /// Exemplo de dados para atualização:
        /// {
        ///   "nome": "Novo Nome",
        ///   "senhaAtual": "senha123",
        ///   "novaSenha": "novaSenha456"
        /// }
        /// </remarks>
        /// <param name="id">ID do administrador que será atualizado</param>
        /// <param name="adminUpdateDto">Dados para atualização do administrador</param>
        /// <response code="204">Atualização realizada com sucesso</response>
        /// <response code="400">Dados inválidos (senha atual incorreta ou nova senha não atende aos requisitos)</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="403">Usuário não tem permissão para editar este administrador</response>
        /// <response code="404">Administrador não encontrado no sistema</response>
        [HttpPatch("{id}/editar")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [SwaggerRequestExample(typeof(AdminUpdateDto), typeof(AdminUpdateDtoExample))]
        public async Task<IActionResult> EditarAdmin(int id, [FromBody] AdminUpdateDto adminUpdateDto)
        {
            // TODO: Adicionar verificação se o admin autenticado é o mesmo do {id} ou tem permissão para editar outros.
            // var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (currentAdminId != id.ToString()) return Forbid();
            try
            {
                await _adminService.UpdateAsync(id, adminUpdateDto);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { errors = new { general = ex.Message } });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { errors = new { admin = ex.Message } });
            }
        }

        /// <summary>
        /// Remove permanentemente um administrador do sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite a exclusão definitiva de um administrador do sistema.
        /// 
        /// Características:
        /// - Exclusão permanente (não é possível recuperar os dados)
        /// - Requer autenticação via token JWT
        /// - Acesso restrito apenas para outros administradores
        /// 
        /// O que acontece:
        /// - Todos os dados do administrador são removidos do banco de dados
        /// - Histórico de atividades é mantido por questões de auditoria
        /// - O administrador não poderá mais acessar o sistema com este ID
        /// 
        /// Exemplo de uso:
        /// - Faça uma requisição DELETE para /api/admins/{id}
        /// 
        /// Observações:
        /// - A exclusão é irreversível
        /// - Recomenda-se fazer backup dos dados importantes antes
        /// - Token JWT deve ser enviado no header da requisição
        /// </remarks>
        /// <param name="id">ID do administrador a ser excluído</param>
        /// <response code="204">Exclusão realizada com sucesso</response>
        /// <response code="401">Token JWT inválido ou não fornecido</response>
        /// <response code="404">Administrador não encontrado no sistema</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletarAdmin(int id)
        {
            try
            {
                await _adminService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { errors = new { admin = ex.Message } });
            }
        }

        /// <summary>
        /// Endpoint para redefinição de senha de administradores.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um administrador redefina sua senha quando esqueceu a senha atual.
        /// 
        /// Como funciona:
        /// 1. O administrador fornece seu e-mail cadastrado
        /// 2. Informa a nova senha desejada
        /// 3. Confirma a nova senha
        /// 
        /// Características:
        /// - Não requer autenticação (acesso público)
        /// - Valida se o e-mail existe no sistema
        /// - Verifica se a senha e confirmação são iguais
        /// - Aplica as regras de segurança para senhas
        /// 
        /// Exemplo de uso:
        /// POST /api/admins/esqueceu-senha
        /// {
        ///     "email": "admin@exemplo.com",
        ///     "novaSenha": "Senha123!",
        ///     "confirmacaoSenha": "Senha123!"
        /// }
        /// 
        /// Observações:
        /// - A senha deve seguir as políticas de segurança do sistema
        /// - O e-mail deve estar cadastrado no sistema
        /// - A confirmação deve ser idêntica à nova senha
        /// </remarks>
        /// <param name="dto">Objeto contendo e-mail, nova senha e confirmação</param>
        /// <response code="200">Senha redefinida com sucesso</response>
        /// <response code="400">Dados inválidos ou senhas não conferem</response>
        /// <response code="404">E-mail não encontrado no sistema</response>
        [HttpPost("esqueceu-senha")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [SwaggerRequestExample(typeof(AdminForgotPasswordDto), typeof(AdminForgotPasswordDtoExample))]
        public async Task<IActionResult> ForgotPassword([FromBody] AdminForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _adminService.ForgotPasswordAsync(dto);
                return Ok(new { message = "Senha redefinida com sucesso." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Nenhum administrador encontrado com este e-mail." });
            }
            catch (ArgumentException ex) // Para senhas diferentes ou outras validações do serviço
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 
