using System;
using System.Security.Cryptography;
using System.Text;
// using System.IdentityModel.Tokens.Jwt; // Não mais usado para gerar token de usuário comum
// using Microsoft.IdentityModel.Tokens; // Não mais usado para gerar token de usuário comum
// using System.Security.Claims; // Não mais usado para gerar token de usuário comum
using System.Threading.Tasks;
using AutoMapper;
using DisasterLink_API.DTOs;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Interfaces.Services;
using Microsoft.Extensions.Configuration; // Ainda pode ser útil para outras configs
using DisasterLink_API.DTOs.Auth; // Adicionar este using
using Google.Apis.Auth; // Adicionar para GoogleJsonWebSignature
using System.Collections.Generic; // Para List em GoogleJsonWebSignature.ValidateAsync

namespace DisasterLink_API.Services
{
    /// <summary>
    /// Implementa lógica de negócio para usuários
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration; // Adicionar IConfiguration para pegar o ClientId

        public UsuarioService(IUsuarioRepository repo, IMapper mapper, IConfiguration configuration)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration; // Adicionar esta linha
        }

        public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto)
        {
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new ApplicationException("Email já cadastrado.");

            var entity = _mapper.Map<Usuario>(dto);
            entity.SenhaHash = HashPassword(dto.Senha);
            // entity.Municipio = dto.Municipio; // Já mapeado pelo AutoMapper se o nome corresponder
            var created = await _repo.AddAsync(entity);
            return _mapper.Map<UsuarioDto>(created);
        }

        public async Task<LoginUsuarioResponseDto> LoginAsync(LoginDto dto) // Tipo de retorno alterado
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null || user.SenhaHash != HashPassword(dto.Senha))
            {
                // throw new ApplicationException("Email ou senha inválidos.");
                return new LoginUsuarioResponseDto(false, "Email ou senha inválidos.");
            }

            // Não gera token JWT para usuário comum
            return new LoginUsuarioResponseDto(true, "Login bem-sucedido!", _mapper.Map<UsuarioDto>(user));
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user is null ? null : _mapper.Map<UsuarioDto>(user);
        }

        public async Task UpdateAsync(int id, UsuarioUpdateDto dto)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            // Adicionar validação de senha atual se a senha for alterada
            if (!string.IsNullOrEmpty(dto.NovaSenha))
            {
                if (string.IsNullOrEmpty(dto.SenhaAtual) || HashPassword(dto.SenhaAtual) != user.SenhaHash)
                {
                    throw new ApplicationException("Senha atual incorreta ou não fornecida.");
                }
                user.SenhaHash = HashPassword(dto.NovaSenha);
            }

            // Atualizar outros campos
            user.Nome = dto.Nome;
            // Email não deve ser alterado aqui, conforme especificação.
            // Se UsuarioUpdateDto permitir alteração de município, adicionar aqui.
            // user.Municipio = dto.Municipio;
            
            await _repo.UpdateAsync(user);
        }

        public async Task<List<UsuarioDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(u => _mapper.Map<UsuarioDto>(u)).ToList();
        }

        // public async Task<List<UsuarioDto>> GetByMunicipioAsync(string municipio)
        // {
        //     var list = await _repo.GetByMunicipioAsync(municipio);
        //     return list.Select(u => _mapper.Map<UsuarioDto>(u)).ToList();
        // }

        public async Task<List<UsuarioDto>> GetByLocalidadeAsync(string? cidadeMunicipio, string? bairro)
        {
            // A validação de que pelo menos um foi fornecido já está no controller.
            var list = await _repo.GetByLocalidadeAsync(cidadeMunicipio, bairro);
            return list.Select(u => _mapper.Map<UsuarioDto>(u)).ToList();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado para exclusão.");

            await _repo.DeleteAsync(id);
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // GenerateCustomToken foi removido pois não é mais usado para usuários comuns

        public async Task ForgotPasswordAsync(UsuarioForgotPasswordDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                // A mensagem de erro específica será tratada no controller
                throw new KeyNotFoundException("Usuário não encontrado."); 
            }

            if (dto.NovaSenha != dto.ConfirmacaoNovaSenha)
            {
                // Esta mensagem será usada pelo controller
                throw new ArgumentException("A confirmação da nova senha não corresponde.");
            }

            // Validação de complexidade da senha (mínimo 6 caracteres) já é feita pelo DTO.
            user.SenhaHash = HashPassword(dto.NovaSenha);
            await _repo.UpdateAsync(user);
        }

        public async Task<LoginUsuarioResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId não configurado.");
                // Valida o token e verifica se o "aud" (audiência/clientId) é o esperado.
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
            }
            catch (InvalidJwtException)
            {
                // Token inválido ou expirado
                // Você pode querer logar ex.ToString() para depuração
                return new LoginUsuarioResponseDto(false, "Token do Google inválido ou expirado.");
            }
            catch (Exception)
            {
                 // Você pode querer logar ex.ToString() para depuração
                return new LoginUsuarioResponseDto(false, "Erro ao validar token do Google.");
            }

            // Tenta encontrar o usuário pelo Google User ID
            var user = await _repo.GetByGoogleUserIdAsync(payload.Subject);

            if (user == null)
            {
                // Se não encontrou pelo Google User ID, tenta pelo email (caso o usuário já exista com cadastro normal)
                user = await _repo.GetByEmailAsync(payload.Email);
                if (user != null)
                {
                    // Usuário encontrado pelo email, vincula o Google User ID
                    user.GoogleUserId = payload.Subject;
                    // SenhaHash pode ser mantida se já existia, ou tornada null se preferir focar no login social
                    // user.SenhaHash = null; 
                }
                else
                {
                    // Novo usuário: cria com os dados do Google
                    user = new Usuario
                    {
                        Email = payload.Email,
                        Nome = payload.Name ?? payload.GivenName ?? "Usuário Google", // Usar nome, ou nome dado, ou um placeholder
                        GoogleUserId = payload.Subject,
                        SenhaHash = null // Não precisa de senha com login do Google
                        // Municipio pode ser deixado como null ou solicitado em um passo posterior no app
                    };
                    await _repo.AddAsync(user); // AddAsync no seu repositório precisa atribuir um ID
                }
            }
            
            if (user.Id == 0 && user.GoogleUserId == payload.Subject) // Checa se o user foi criado e precisa ser recuperado com ID
            {
                 user = await _repo.GetByGoogleUserIdAsync(payload.Subject) ?? throw new ApplicationException("Falha ao recuperar usuário recém-criado.");
            }

            await _repo.UpdateAsync(user); // Garante que o GoogleUserId seja salvo se foi um vínculo

            return new LoginUsuarioResponseDto(true, "Login com Google bem-sucedido!", _mapper.Map<UsuarioDto>(user));
        }
    }
} 