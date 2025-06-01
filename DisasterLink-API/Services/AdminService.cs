using AutoMapper;
using DisasterLink_API.DTOs.Admin;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces.Repositories;
using DisasterLink_API.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using DisasterLink_API.DTOs.Auth;

namespace DisasterLink_API.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminService(IAdminRepository adminRepository, IMapper mapper, IConfiguration configuration)
        {
            _adminRepository = adminRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AdminDto> CreateAsync(AdminCreateDto adminCreateDto)
        {
            if (await _adminRepository.EmailExistsAsync(adminCreateDto.Email))
            {
                throw new ApplicationException("Este email já está cadastrado.");
            }

            var admin = _mapper.Map<Admin>(adminCreateDto);
            admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminCreateDto.Senha);

            // Gerar ID de 4 dígitos aleatório e único para o Admin
            Random random = new Random();
            int newId;
            int attempts = 0;
            const int maxAttempts = 100; // Evitar loop infinito

            do
            {
                if (attempts >= maxAttempts)
                {
                    throw new ApplicationException("Não foi possível gerar um ID de Admin único após várias tentativas.");
                }
                newId = random.Next(1000, 10000); // Gera número entre 1000 e 9999
                attempts++;
            } while (await _adminRepository.IdExistsAsync(newId)); // Supondo que IdExistsAsync exista no repositório
            
            admin.Id = newId;

            var createdAdmin = await _adminRepository.AddAsync(admin);
            return _mapper.Map<AdminDto>(createdAdmin);
        }

        public async Task<AdminLoginResponseDto> LoginAsync(AdminLoginDto adminLoginDto)
        {
            var admin = await _adminRepository.GetByEmailAsync(adminLoginDto.Email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(adminLoginDto.Senha, admin.SenhaHash))
            {
                throw new ApplicationException("Email ou senha inválidos.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Chave JWT não configurada corretamente."));
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Name, admin.Nome),
                    new Claim(ClaimTypes.Role, "Admin") // Adiciona o papel de Admin
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token válido por 7 dias
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new AdminLoginResponseDto
            {
                Token = tokenString,
                Admin = _mapper.Map<AdminDto>(admin),
                Expiration = tokenDescriptor.Expires ?? DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<AdminDto> GetByIdAsync(int id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null) throw new KeyNotFoundException("Administrador não encontrado.");
            return _mapper.Map<AdminDto>(admin);
        }

        public async Task<List<AdminDto>> GetAllAsync()
        {
            var admins = await _adminRepository.GetAllAsync();
            return _mapper.Map<List<AdminDto>>(admins);
        }

        public async Task UpdateAsync(int id, AdminUpdateDto adminUpdateDto)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null) throw new KeyNotFoundException("Administrador não encontrado.");

            // Verificar senha atual
            if (!BCrypt.Net.BCrypt.Verify(adminUpdateDto.SenhaAtual, admin.SenhaHash))
            {
                throw new ApplicationException("Senha atual incorreta.");
            }

            // Atualizar nome se fornecido
            if (!string.IsNullOrWhiteSpace(adminUpdateDto.Nome))
            {
                admin.Nome = adminUpdateDto.Nome;
            }

            // Atualizar senha se fornecida e válida
            if (!string.IsNullOrWhiteSpace(adminUpdateDto.NovaSenha))
            {
                if (adminUpdateDto.NovaSenha != adminUpdateDto.ConfirmacaoNovaSenha)
                {
                    throw new ApplicationException("A nova senha e a confirmação não coincidem.");
                }
                // Adicionar aqui a validação de complexidade para NovaSenha se o DTO não o fizer completamente via atributos
                admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminUpdateDto.NovaSenha);
            }

            await _adminRepository.UpdateAsync(admin);
        }

        public async Task DeleteAsync(int id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null) throw new KeyNotFoundException("Administrador não encontrado para exclusão.");
            
            await _adminRepository.DeleteAsync(id);
        }

        public async Task ForgotPasswordAsync(AdminForgotPasswordDto dto)
        {
            var admin = await _adminRepository.GetByEmailAsync(dto.Email);
            if (admin == null)
            {
                // A mensagem de erro específica será tratada no controller
                throw new KeyNotFoundException("Admin não encontrado."); 
            }

            if (dto.NovaSenha != dto.ConfirmacaoNovaSenha)
            {
                // Esta mensagem será usada pelo controller
                throw new ArgumentException("A confirmação da nova senha não corresponde.");
            }

            // Validação de complexidade da senha (mínimo 6 caracteres) já é feita pelo DTO.
            // Se precisar de mais validações, adicionar aqui.

            admin.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
            await _adminRepository.UpdateAsync(admin);
        }
    }
} 