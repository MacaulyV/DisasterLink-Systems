using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;
using System;

namespace DisasterLink_API.SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para criação de alerta
    /// </summary>
    public class AlertaCreateDtoExample : IExamplesProvider<AlertaCreateDto>
    {
        public AlertaCreateDto GetExamples()
        {
            return new AlertaCreateDto
            {
                Tipo = "Inundação",
                Titulo = "ALERTA DE Enchente",
                Descricao = "Previsão de chuvas intensas nas próximas horas. Risco de Enchente nas Proximidades.",
                Cidade = "Porto Alegre",
                Bairro = "Centro",
                Logradouro = "Avenida Borges de Medeiros, 1500"
            };
        }
    }
} 