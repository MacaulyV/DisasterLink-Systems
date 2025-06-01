using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;
using System.Collections.Generic;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para atualização parcial (PATCH) de um Abrigo Temporário.
    /// Mostra a alteração de apenas alguns campos.
    /// </summary>
    public class AbrigoTemporarioPatchDtoExample : IExamplesProvider<AbrigoTemporarioCreateDto>
    {
        public AbrigoTemporarioCreateDto GetExamples()
        {
            // Para PATCH, idealmente teríamos um DTO com todos os campos opcionais.
            // Como estamos usando AbrigoTemporarioCreateDto, o exemplo abaixo
            // demonstra a intenção de um PATCH, mas na prática, o controller/serviço
            // precisaria lidar com os campos não fornecidos adequadamente (e.g., não atualizá-los).
            // No Swagger UI, este exemplo será apresentado para a operação PATCH.
            return new AbrigoTemporarioCreateDto
            {
                // Nome não será alterado, então poderia ser omitido em um DTO de PATCH real
                // Para AbrigoTemporarioCreateDto, se omitido, causaria erro de validação se fosse [Required]
                // mas para o propósito do exemplo de PATCH, mostramos como seria.
                // Nome = null, // Exemplo: Não alterar nome

                Descricao = "Atualização: Abrigo agora com maior capacidade de atendimento e novas instalações sanitárias.",
                Capacidade = 75,
                // Outros campos como CidadeMunicipio, Bairro, Logradouro poderiam ser omitidos
                // se não fossem Required no AbrigoTemporarioCreateDto.
                // Para um PATCH real, você enviaria APENAS os campos que quer mudar.
                
                // ImagemUrls também pode ser atualizado, por exemplo, adicionando ou removendo uma imagem.
                // Se não for fornecido, não deve ser alterado.
                ImagemUrls = new List<string> 
                {
                    "https://storage.disasterlink.com.br/abrigos/abrigo_esperanca_1_atualizado.jpg"
                }
                // Campos como Nome, CidadeMunicipio, Bairro, Logradouro são [Required] no CreateDto.
                // Para um PATCH real, um UpdateDto com campos opcionais seria melhor.
                // Para este exemplo, vamos preenchê-los para que o exemplo seja 'válido' contra o CreateDto,
                // mas com a intenção de mostrar uma atualização parcial.
                , Nome = "Abrigo Comunitário Esperança (Nome Inalterado)",
                CidadeMunicipio = "Nova Esperança (Inalterado)",
                Bairro = "Feliz (Inalterado)",
                Logradouro = "Rua da Escola, 500 (Inalterado)"
            };
        }
    }
} 