using DisasterLink_API.Entities;
// using DisasterLink_API.MLModels; // Removido, pois CapacidadePontoColetaInput foi deletado
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterLink_API.Data
{
    public static class DataSeeder
    {
        private static readonly Random _random = new Random();

        // generateTrainingData não é mais necessário para o modelo antigo
        public static async Task SeedDatabaseAsync(DisasterLinkDbContext context, IServiceProvider serviceProvider)
        {
            if (!await context.PontosDeColetaDeDoacoes.AnyAsync())
            {
                await SeedPontosDeColetaAsync(context);
            }
            // Os dados dos pontos de coleta agora servirão apenas para alimentar o modelo ML.NET
        }

        private static async Task SeedPontosDeColetaAsync(DisasterLinkDbContext context)
        {
            var cidades = new[] { "São Paulo", "Rio de Janeiro", "Campinas", "Belo Horizonte", "Porto Alegre", "Curitiba" };
            var bairrosPorCidade = new Dictionary<string, string[]>
            {
                { "São Paulo", new[] { "Mooca", "Pinheiros", "Tatuapé", "Santana", "Liberdade" } },
                { "Rio de Janeiro", new[] { "Copacabana", "Ipanema", "Barra da Tijuca", "Botafogo", "Flamengo" } },
                { "Campinas", new[] { "Centro", "Cambuí", "Taquaral", "Jardim Progresso", "Barão Geraldo" } },
                { "Belo Horizonte", new[] { "Savassi", "Lourdes", "Pampulha", "Centro", "Funcionários" } },
                { "Porto Alegre", new[] { "Moinhos de Vento", "Bom Fim", "Cidade Baixa", "Menino Deus", "Auxiliadora" } },
                { "Curitiba", new[] { "Batel", "Centro Cívico", "Água Verde", "Santa Felicidade", "Cabral" } }
            };

            var tiposPontoColeta = new[] { "Alimentos", "Roupas", "Kits de Higiene", "Água Potável", "Kits Infantis", "Medicamentos" };
            var itensPorTipo = new Dictionary<string, string[]>
            {
                { "Alimentos", new[] { "arroz", "feijão", "macarrão", "óleo", "leite em pó", "cesta básica" } },
                { "Roupas", new[] { "agasalho", "camiseta", "calça", "meias", "cobertor", "roupa de inverno" } },
                { "Kits de Higiene", new[] { "sabonete", "pasta de dente", "escova de dente", "papel higiênico", "absorvente" } },
                { "Água Potável", new[] { "água mineral", "galão de água", "purificador de água" } },
                { "Kits Infantis", new[] { "fraldas", "leite em pó infantil", "lenços umedecidos", "roupas de bebê" } },
                { "Medicamentos", new[] { "analgésico", "antitérmico", "curativos", "gaze", "álcool" } }
            };

            var pontos = new List<PontoDeColetaDeDoacoes>();
            int pontoIdCounter = 1000; 

            for (int i = 0; i < 1000; i++) 
            {
                var cidade = cidades[_random.Next(cidades.Length)];
                var bairrosDisponiveis = bairrosPorCidade[cidade];
                var bairro = bairrosDisponiveis[_random.Next(bairrosDisponiveis.Length)];
                var tipo = tiposPontoColeta[_random.Next(tiposPontoColeta.Length)];
                var itensDisponiveis = itensPorTipo[tipo];
                
                // Gerar entre 1 e 5 itens para melhor diferenciação
                var numItensGerar = _random.Next(1, 6); 
                var estoqueList = new List<string>();
                for(int j=0; j < numItensGerar; j++)
                {
                    estoqueList.Add(itensDisponiveis[_random.Next(itensDisponiveis.Length)]);
                }
                string estoqueString = string.Join(", ", estoqueList.Distinct());

                pontos.Add(new PontoDeColetaDeDoacoes
                {
                    Id = pontoIdCounter++,
                    Tipo = tipo,
                    Descricao = $"Ponto de coleta de {tipo.ToLower()} no bairro {bairro}",
                    DataInicio = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                    Cidade = cidade,
                    Bairro = bairro,
                    Logradouro = $"Rua {GetRandomStreetName()} {GetRandomStreetType()}, {_random.Next(1, 500)}",
                    Estoque = estoqueString, 
                    ImagemUrls = null 
                });
            }
            await context.PontosDeColetaDeDoacoes.AddRangeAsync(pontos);
            await context.SaveChangesAsync();
            Console.WriteLine($"{pontos.Count} pontos de coleta foram semeados no banco de dados.");
        }

        private static string GetRandomStreetName()
        {
            string[] names = { "Flores", "Principal", "Alegria", "Esperança", "União", "Amizade", "Vitória", "Paz" };
            return names[_random.Next(names.Length)];
        }

        private static string GetRandomStreetType()
        {
            string[] types = { "Rua", "Avenida", "Travessa", "Alameda" };
            return types[_random.Next(types.Length)];
        }

        // O método GenerateCapacidadeTrainingSamplesAsync foi removido.
    }
} 