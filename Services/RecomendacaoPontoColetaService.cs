using DisasterLink_API.Data;
using DisasterLink_API.Entities;
using DisasterLink_API.Interfaces;
using DisasterLink_API.MLModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DisasterLink_API.Services
{
    public class RecomendacaoPontoColetaService : IRecomendacaoPontoColetaService
    {
        private readonly DisasterLinkDbContext _context;
        private readonly ModeloRecomendacaoPontoColeta _modeloRecomendacao;
        private bool _modeloInicializado = false;

        public RecomendacaoPontoColetaService(DisasterLinkDbContext context)
        {
            _context = context;
            _modeloRecomendacao = new ModeloRecomendacaoPontoColeta();
        }

        public async Task InicializarModeloAsync()
        {
            if (_modeloInicializado) return;

            try
            {
                if (!_modeloRecomendacao.ModeloExiste())
                {
                    Console.WriteLine("Modelo de ML (.zip) não encontrado. Tentando treinar um novo modelo...");
                    var pontosDeColeta = await ObterPontosDeColetaAsync();
                    if (pontosDeColeta.Any())
                    {
                        _modeloRecomendacao.TreinarEsalvarModelo(pontosDeColeta);
                        Console.WriteLine("Novo modelo de ML treinado e salvo com sucesso.");
                        _modeloRecomendacao.CarregarModelo();
                    }
                    else
                    {
                        Console.WriteLine("Não há dados de pontos de coleta para treinar um novo modelo. O sistema de recomendação pode não funcionar como esperado.");
                        // Opcionalmente, poderia lançar uma exceção ou lidar com isso de outra forma
                    }
                }
                else
                {
                    Console.WriteLine("Carregando modelo de ML (.zip) existente...");
                    _modeloRecomendacao.CarregarModelo();
                }
                _modeloInicializado = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro crítico ao inicializar o modelo de recomendação: {ex.Message}");
                // Considere registrar o erro detalhadamente
                // Em um cenário de produção, você pode querer ter uma estratégia de fallback ou notificação
                throw; // Ou lidar de forma diferente para não impedir a API de iniciar, se apropriado
            }
        }

        private async Task GarantirModeloInicializadoAsync()
        {
            if (!_modeloInicializado)
            {
                await InicializarModeloAsync();
            }
        }

        private async Task<List<PontoDeColetaDeDoacoes>> ObterPontosDeColetaAsync()
        {
            return await _context.PontosDeColetaDeDoacoes
                .AsNoTracking()
                .ToListAsync();
        }

        // Este método é chamado pelo endpoint de treinamento explícito
        public async Task AtualizarModeloAsync()
        {
            Console.WriteLine("Iniciando o retreinamento explícito do modelo de recomendação...");
            var pontosDeColeta = await ObterPontosDeColetaAsync();
            
            if (!pontosDeColeta.Any())
            {
                Console.WriteLine("AVISO: Não há pontos de coleta no banco de dados para treinar o modelo.");
                // Decide se quer criar um modelo 'vazio' ou lançar erro.
                // Por ora, vamos apenas logar e não salvar um novo modelo se não houver dados.
                return; 
            }
            
            _modeloRecomendacao.TreinarEsalvarModelo(pontosDeColeta);
            Console.WriteLine("Modelo de ML retreinado e salvo com sucesso.");
            // Recarregar o modelo na memória após o treinamento
            _modeloRecomendacao.CarregarModelo(); 
            _modeloInicializado = true; // Garante que está marcado como inicializado
        }

        public async Task<List<PontoColetaResultado>> ObterRecomendacoesAsync(string necessidade, string? cidade = null)
        {
            await GarantirModeloInicializadoAsync();

            if (!_modeloInicializado || _modeloRecomendacao == null) // Checagem extra
            {
                Console.WriteLine("Erro: Modelo de recomendação não está pronto para uso.");
                return new List<PontoColetaResultado>(); // Retorna lista vazia ou lança exceção
            }

            var solicitacao = new PontoColetaRecomendacao
            {
                Necessidade = necessidade,
                Cidade = cidade
            };
             
            var pontosDeColeta = await ObterPontosDeColetaAsync(); // Obtém todos para o modelo fazer a predição
            return _modeloRecomendacao.ObterRecomendacoes(pontosDeColeta, solicitacao);
        }
    }
} 