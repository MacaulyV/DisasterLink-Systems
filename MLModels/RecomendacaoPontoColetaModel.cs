using DisasterLink_API.Entities;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DisasterLink_API.MLModels
{
    // Modelo de entrada para treinamento e predição
    public class PontoColetaInput
    {
        [LoadColumn(0)]
        public string Necessidade { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string Cidade { get; set; } = string.Empty;

        [LoadColumn(2)]
        public int PontoId { get; set; }

        [LoadColumn(3)]
        public string Tipo { get; set; } = string.Empty;

        [LoadColumn(4)]
        public string Descricao { get; set; } = string.Empty;

        [LoadColumn(5)]
        public string Estoque { get; set; } = string.Empty;

        [LoadColumn(6)]
        public string Bairro { get; set; } = string.Empty;

        [LoadColumn(7)]
        public string Logradouro { get; set; } = string.Empty;

        // ImagemUrls é armazenado como string? no banco de dados (representação JSON de URLs)
        public string? ImagemUrls { get; set; }

        [LoadColumn(8)]
        [ColumnName("Label")]
        public bool EhRelevante { get; set; }
    }

    // Modelo de saída da predição
    public class PontoColetaPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool EhRelevante { get; set; }

        [ColumnName("Probability")]
        public float Score { get; set; }
    }

    // Resultado para retornar ao usuário
    public class PontoColetaResultado
    {
        public int PontoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Estoque { get; set; } = string.Empty;
        public string? ImagemUrls { get; set; }
        public double Score { get; set; }
    }

    // Parâmetros para recomendação
    public class PontoColetaRecomendacao
    {
        public string Necessidade { get; set; } = string.Empty;
        public string? Cidade { get; set; }
    }

    public class ModeloRecomendacaoPontoColeta
    {
        private readonly string _modelPath;
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<PontoColetaInput, PontoColetaPrediction>? _predictionEngine;
        private readonly Dictionary<string, HashSet<string>> _mapeamentoTermos;

        public ModeloRecomendacaoPontoColeta()
        {
            _mlContext = new MLContext(seed: 42);
            _modelPath = Path.Combine(AppContext.BaseDirectory, "MLModels", "ModeloPontoColeta.zip");
            _mapeamentoTermos = CriarMapeamentoTermos();
        }

        private Dictionary<string, HashSet<string>> CriarMapeamentoTermos()
        {
            var mapeamento = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            mapeamento["Alimentos"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "comida", "refeição", "mantimentos", "alimentação", "arroz", "feijão", 
                "macarrão", "óleo", "leite em pó", "cesta básica", "alimento", "comestível",
                "cereal", "grão", "enlatado", "conserva", "não perecível", "farinha",
                "açúcar", "sal", "café", "chá", "biscoito", "bolacha", "alimento não perecível"
            };
            mapeamento["Roupas"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { 
                "vestuário", "vestimenta", "agasalho", "camiseta", "calça", "meias", 
                "cobertor", "roupa de inverno", "peças de roupa", "blusa", "casaco",
                "jaqueta", "tênis", "sapato", "calçado", "roupa", "peça", "vestimenta",
                "manta", "lençol", "toalha", "roupa de cama", "edredom", "travesseiro"
            };
            mapeamento["Kits de Higiene"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { 
                "higiene", "produtos de higiene", "kit higiênico", "sabonete", "pasta de dente", 
                "escova de dente", "papel higiênico", "absorvente", "higiene pessoal",
                "xampu", "shampoo", "condicionador", "desodorante", "creme dental",
                "fio dental", "algodão", "álcool em gel", "sabão", "detergente",
                "material de limpeza", "produto de limpeza", "artigo de higiene"
            };
            mapeamento["Água Potável"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { 
                "água", "água mineral", "galão de água", "purificador de água", "água para beber",
                "água tratada", "garrafão", "garrafa de água", "água engarrafada", "água filtrada",
                "garrafinha", "água potável", "hidratação", "líquido", "bebida"
            };
            mapeamento["Kits Infantis"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { 
                "itens para bebê", "itens infantis", "produtos para crianças", "fraldas", 
                "leite em pó infantil", "lenços umedecidos", "roupas de bebê", "itens para criança",
                "fralda", "mamadeira", "chupeta", "leite infantil", "roupa de bebê",
                "brinquedo", "papinha", "comida de bebê", "carrinho", "berço", "fórmula infantil",
                "pomada para assadura", "cobertor infantil", "manta para bebê", "item infantil",
                "produto para bebê", "artigo infantil", "item para criança"
            };
            mapeamento["Medicamentos"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { 
                "remédios", "medicação", "produtos farmacêuticos", "analgésico", "antitérmico", 
                "curativos", "gaze", "álcool", "farmácia", "remédio", "medicamento",
                "anti-inflamatório", "antibiótico", "band-aid", "curativo", "esparadrapo",
                "produto farmacêutico", "soro", "dipirona", "paracetamol", "ibuprofeno",
                "anti-alérgico", "pomada", "termômetro", "medidor de pressão", "produto médico",
                "material hospitalar", "primeiros socorros", "kit médico"
            };
            return mapeamento;
        }

        public bool ModeloExiste()
        {
            return File.Exists(_modelPath);
        }

        public void CarregarModelo()
        {
            if (!ModeloExiste())
            {
                throw new FileNotFoundException($"O modelo de ML não foi encontrado em: {_modelPath}");
            }

            _model = _mlContext.Model.Load(_modelPath, out _);
            if (_model == null) throw new InvalidOperationException("Falha ao carregar o modelo de ML.");
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<PontoColetaInput, PontoColetaPrediction>(_model);
        }

        public void TreinarEsalvarModelo(List<PontoDeColetaDeDoacoes> pontosDeColeta)
        {
            Console.WriteLine($"Iniciando treinamento do modelo com {pontosDeColeta.Count} pontos de coleta");

            // Cria diretório se não existir
            var diretorio = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }

            // Preparar dados de treinamento
            var dadosTreinamento = PrepararDadosTreinamento(pontosDeColeta);
            if (!dadosTreinamento.Any()) 
            {
                 Console.WriteLine("Nenhum dado de treinamento preparado. O modelo não será treinado.");
                 return;
            }
            var dadosView = _mlContext.Data.LoadFromEnumerable(dadosTreinamento);

            // Pipeline de treinamento
            var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "NecessidadeFeatures",
                    inputColumnName: "Necessidade")
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "TipoFeatures",
                    inputColumnName: "Tipo"))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "EstoqueFeatures",
                    inputColumnName: "Estoque"))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "DescricaoFeatures",
                    inputColumnName: "Descricao"))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "CidadeFeatures",
                    inputColumnName: "Cidade"))
                // Aumentando o peso das características relacionadas à necessidade e tipo
                .Append(_mlContext.Transforms.Concatenate(
                    outputColumnName: "WeightedFeatures",
                    "NecessidadeFeatures", "NecessidadeFeatures", // duplicado para dar mais peso
                    "TipoFeatures", "TipoFeatures", "TipoFeatures", // triplicado para dar mais peso
                    "EstoqueFeatures", "EstoqueFeatures", // duplicado para dar mais peso
                    "DescricaoFeatures", 
                    "CidadeFeatures", "CidadeFeatures")) // duplicado para manter a prioridade da cidade
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "WeightedFeatures"));

            // Treinar o modelo
            Console.WriteLine("Treinando o modelo de ML...");
            _model = pipeline.Fit(dadosView);

            // Salvar o modelo
            _mlContext.Model.Save(_model, dadosView.Schema, _modelPath);
            Console.WriteLine($"Modelo salvo em: {_modelPath}");

            // Criar o prediction engine
            if (_model == null) throw new InvalidOperationException("Falha ao treinar o modelo de ML.");
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<PontoColetaInput, PontoColetaPrediction>(_model);
        }

        private List<PontoColetaInput> PrepararDadosTreinamento(List<PontoDeColetaDeDoacoes> pontosDeColeta)
        {
            var dadosTreinamento = new List<PontoColetaInput>();
            
            // Criamos mais exemplos positivos para tipos específicos para balancear o dataset
            foreach (var ponto in pontosDeColeta)
            {
                // Exemplo positivo forte: termo de busca exatamente igual ao tipo
                var tipo = ponto.Tipo;
                dadosTreinamento.Add(new PontoColetaInput
                {
                    Necessidade = tipo, // Necessidade exata = tipo do ponto
                    Cidade = ponto.Cidade,
                    PontoId = ponto.Id,
                    Tipo = ponto.Tipo,
                    Descricao = ponto.Descricao,
                    Estoque = ponto.Estoque ?? string.Empty,
                    Bairro = ponto.Bairro,
                    Logradouro = ponto.Logradouro,
                    ImagemUrls = ponto.ImagemUrls,
                    EhRelevante = true
                });
                
                // Duplicar este exemplo para dar mais peso à correspondência exata
                dadosTreinamento.Add(new PontoColetaInput
                {
                    Necessidade = tipo, 
                    Cidade = ponto.Cidade,
                    PontoId = ponto.Id,
                    Tipo = ponto.Tipo,
                    Descricao = ponto.Descricao,
                    Estoque = ponto.Estoque ?? string.Empty,
                    Bairro = ponto.Bairro,
                    Logradouro = ponto.Logradouro,
                    ImagemUrls = ponto.ImagemUrls,
                    EhRelevante = true
                });
                
                // Adicionar exemplo positivo para cada item em estoque
                if (!string.IsNullOrEmpty(ponto.Estoque))
                {
                    foreach (var item in ponto.Estoque.Split(',').Select(i => i.Trim()))
                    {
                        if (item.Length > 2) 
                        {
                            dadosTreinamento.Add(new PontoColetaInput
                            {
                                Necessidade = item, // Item específico do estoque
                                Cidade = ponto.Cidade,
                                PontoId = ponto.Id,
                                Tipo = ponto.Tipo,
                                Descricao = ponto.Descricao,
                                Estoque = ponto.Estoque,
                                Bairro = ponto.Bairro,
                                Logradouro = ponto.Logradouro,
                                ImagemUrls = ponto.ImagemUrls,
                                EhRelevante = true
                            });
                            
                            // Duplicar exemplos de correspondência com estoque também
                            dadosTreinamento.Add(new PontoColetaInput
                            {
                                Necessidade = item,
                                Cidade = ponto.Cidade,
                                PontoId = ponto.Id,
                                Tipo = ponto.Tipo,
                                Descricao = ponto.Descricao,
                                Estoque = ponto.Estoque,
                                Bairro = ponto.Bairro,
                                Logradouro = ponto.Logradouro,
                                ImagemUrls = ponto.ImagemUrls,
                                EhRelevante = true
                            });
                        }
                    }
                }
                
                // Adicionar exemplos para termos relacionados
                if (_mapeamentoTermos.TryGetValue(ponto.Tipo, out var termos))
                {
                    foreach (var termo in termos)
                    {
                        dadosTreinamento.Add(new PontoColetaInput
                        {
                            Necessidade = termo, // Termo relacionado ao tipo
                            Cidade = ponto.Cidade,
                            PontoId = ponto.Id,
                            Tipo = ponto.Tipo,
                            Descricao = ponto.Descricao,
                            Estoque = ponto.Estoque ?? string.Empty,
                            Bairro = ponto.Bairro,
                            Logradouro = ponto.Logradouro,
                            ImagemUrls = ponto.ImagemUrls,
                            EhRelevante = true
                        });
                    }
                }
                
                // Exemplos negativos: pontos de tipo diferente da necessidade
                var outrosTipos = pontosDeColeta
                    .Where(p => !p.Tipo.Equals(ponto.Tipo, StringComparison.OrdinalIgnoreCase) &&
                               p.Cidade.Equals(ponto.Cidade, StringComparison.OrdinalIgnoreCase))
                    .Take(2);
                
                foreach (var outroPonto in outrosTipos)
                {
                    dadosTreinamento.Add(new PontoColetaInput
                    {
                        Necessidade = ponto.Tipo, // Necessidade = tipo deste ponto
                        Cidade = ponto.Cidade,
                        PontoId = outroPonto.Id, // Mas dados do outro ponto
                        Tipo = outroPonto.Tipo,
                        Descricao = outroPonto.Descricao,
                        Estoque = outroPonto.Estoque ?? string.Empty,
                        Bairro = outroPonto.Bairro,
                        Logradouro = outroPonto.Logradouro,
                        ImagemUrls = outroPonto.ImagemUrls,
                        EhRelevante = false // Não é relevante porque o tipo não corresponde
                    });
                }
            }
            
            Console.WriteLine($"Preparados {dadosTreinamento.Count} exemplos para treinamento");
            return dadosTreinamento;
        }

        public List<PontoColetaResultado> ObterRecomendacoes(List<PontoDeColetaDeDoacoes> pontosDeColeta, PontoColetaRecomendacao solicitacao)
        {
            if (_predictionEngine == null)
            {
                // Tenta carregar o modelo se ele ainda não foi carregado.
                if (ModeloExiste()) {
                    CarregarModelo();
                } else {
                    // Se o modelo não existe e não pode ser carregado, não há como fazer predições.
                    Console.WriteLine("Motor de predição não está pronto e modelo não existe para ser carregado.");
                    return new List<PontoColetaResultado>();
                }
                if (_predictionEngine == null) {
                    Console.WriteLine("Falha crítica ao preparar o motor de predição.");
                    return new List<PontoColetaResultado>();
                }
            }

            var resultados = new List<PontoColetaResultado>();

            // Normalizar a necessidade
            var necessidadeNormalizada = NormalizarNecessidade(solicitacao.Necessidade);

            // Filtrar pontos por cidade se especificada
            var pontosFiltrados = pontosDeColeta;
            if (!string.IsNullOrWhiteSpace(solicitacao.Cidade))
            {
                pontosFiltrados = pontosDeColeta
                    .Where(p => p.Cidade.Equals(solicitacao.Cidade, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            const double MaxTheoreticalScore = 2.0; // 1.0 (modelo) + 0.5 (tipo) + 0.3 (estoque) + 0.2 (termo relacionado)

            // Aplicar o modelo para cada ponto de coleta
            foreach (var ponto in pontosFiltrados)
            {
                // Pontuação manual para relevância direta do tipo
                double scoreAdicional = 0;
                
                // Correspondência direta entre necessidade e tipo (peso maior)
                if (ponto.Tipo.Equals(necessidadeNormalizada, StringComparison.OrdinalIgnoreCase))
                {
                    scoreAdicional += 0.5; // Peso alto para correspondência direta de tipo
                }
                
                // Correspondência entre necessidade e estoque
                if (!string.IsNullOrEmpty(ponto.Estoque) && 
                    ponto.Estoque.Contains(solicitacao.Necessidade, StringComparison.OrdinalIgnoreCase))
                {
                    scoreAdicional += 0.3; // Peso significativo para item exato no estoque
                }
                
                // Correspondência com termos relacionados
                if (_mapeamentoTermos.TryGetValue(ponto.Tipo, out var termos) &&
                    termos.Contains(solicitacao.Necessidade))
                {
                    scoreAdicional += 0.2; // Peso para termos relacionados
                }

                var input = new PontoColetaInput
                {
                    Necessidade = solicitacao.Necessidade,
                    Cidade = solicitacao.Cidade ?? string.Empty,
                    PontoId = ponto.Id,
                    Tipo = ponto.Tipo,
                    Descricao = ponto.Descricao,
                    Estoque = ponto.Estoque ?? string.Empty,
                    Bairro = ponto.Bairro,
                    Logradouro = ponto.Logradouro,
                    ImagemUrls = ponto.ImagemUrls
                };

                var predicao = _predictionEngine.Predict(input);
                
                // Combinar score do modelo com o score manual
                double scoreTotal = predicao.Score + scoreAdicional;

                // Normalizar o score para uma escala de 0-100
                double scoreNormalizado = (scoreTotal / MaxTheoreticalScore) * 100.0;
                scoreNormalizado = Math.Min(scoreNormalizado, 100.0); // Cap em 100
                scoreNormalizado = Math.Max(scoreNormalizado, 0.0);   // Floor em 0
                scoreNormalizado = Math.Round(scoreNormalizado, 1); // Arredondar para uma casa decimal

                // Adicionar apenas se for relevante ou tiver score total original acima de um limiar
                if (predicao.EhRelevante || scoreTotal > 0.3) // Limiar original para relevância
                {
                    resultados.Add(new PontoColetaResultado
                    {
                        PontoId = ponto.Id,
                        Tipo = ponto.Tipo,
                        Descricao = ponto.Descricao,
                        Cidade = ponto.Cidade,
                        Bairro = ponto.Bairro,
                        Logradouro = ponto.Logradouro,
                        Estoque = ponto.Estoque ?? string.Empty,
                        ImagemUrls = ponto.ImagemUrls,
                        Score = scoreNormalizado // Usamos o score normalizado
                    });
                }
            }

            // Ordenar por score e limitar a 10 resultados
            return resultados
                .OrderByDescending(r => r.Score)
                .Take(10)
                .ToList();
        }
        
        private string NormalizarNecessidade(string necessidade)
        {
            // Normalizar a necessidade para corresponder aos tipos de pontos de coleta
            var necessidadeLower = necessidade.ToLowerInvariant();
            
            foreach (var kvp in _mapeamentoTermos)
            {
                string tipo = kvp.Key;
                HashSet<string> termos = kvp.Value;
                
                // Verificar correspondência direta com o tipo
                if (tipo.Equals(necessidade, StringComparison.OrdinalIgnoreCase))
                    return tipo;
                
                // Verificar se necessidade está entre os termos relacionados
                if (termos.Contains(necessidadeLower))
                    return tipo;
                
                // Verificar correspondência parcial com tipo
                if (tipo.Contains(necessidade, StringComparison.OrdinalIgnoreCase) || 
                    necessidade.Contains(tipo, StringComparison.OrdinalIgnoreCase))
                    return tipo;
            }
            
            return necessidade; // Retorna original se não encontrar correspondência
        }
    }
} 