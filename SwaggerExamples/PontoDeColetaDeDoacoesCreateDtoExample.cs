using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs.Create;
using System.Collections.Generic;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para criação de um Ponto de Coleta de Doações.
    /// </summary>
    public class PontoDeColetaDeDoacoesCreateDtoExample : IExamplesProvider<PontoDeColetaDeDoacoesCreateDto>
    {
        public PontoDeColetaDeDoacoesCreateDto GetExamples()
        {
            return new PontoDeColetaDeDoacoesCreateDto
            {
                Tipo = "Arrecadação de Alimentos Não Perecíveis",
                Descricao = "Ponto de coleta para alimentos não perecíveis, água potável e produtos de higiene pessoal destinados às vítimas da enchente.",
                Cidade = "Porto Alegre",
                Bairro = "Centro Histórico",
                Logradouro = "Praça da Matriz, 1 (Em frente à Catedral)",
                ImagemUrls = new List<string>
                {
                    "https://example.com/images/ponto_coleta_alimentos_1.jpg",
                    "https://example.com/images/ponto_coleta_alimentos_2.jpg"
                },
                Estoque = "cestas básicas"
            };
        }
    }
} 