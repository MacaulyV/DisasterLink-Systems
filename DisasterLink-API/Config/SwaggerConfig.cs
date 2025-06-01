using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Reflection;

namespace DisasterLink_API.Config
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DisasterLink API",
                    Version = "v1",
                    Description = "API .NET Core para gerenciamento de ocorrências, alertas, campanhas e usuários.\n" +
                                   "- Ocorrências: CRUD, filtragem e priorização via ML.NET (a implementar)\n" +
                                   "- Alertas: envio e consulta\n" +
                                   "- Campanhas: criação, participação e listagem de participantes\n" +
                                   "- Usuários: cadastro, login, perfil\n" +
                                   "- Documentação HATEOAS e exemplos práticos\n" +
                                   "- Rate Limiting e tratamento global de erros",
                    Contact = new OpenApiContact
                    {
                        Name = "Repositorio DisasterLink Systems",
                        Url = new Uri("https://github.com/MacaulyV/DisasterLink-Systems"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    },
                    // ExternalDocs não suportado diretamente aqui; usaremos DocumentFilter
                });

                // Inclui comentários XML
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

                // Habilita exemplos de request/response
                c.ExampleFilters();

                // Ordena tags alfabeticamente
                c.DocumentFilter<SwaggerTagsSorter>();
            });

            // Registra exemplos de Swagger
            services.AddSwaggerExamplesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static void UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DisasterLink API v1");
                c.RoutePrefix = string.Empty; // serve UI na raiz
                c.InjectStylesheet("/swagger-ui.css");
                c.InjectJavascript("/swagger-custom.js");
                // colapsa operações por padrão e oculta seção de schemas
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
            });
        }
    }
} 