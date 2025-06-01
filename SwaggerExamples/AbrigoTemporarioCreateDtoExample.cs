using Swashbuckle.AspNetCore.Filters;
using DisasterLink_API.DTOs;
using System.Collections.Generic;

namespace DisasterLink_API.SwaggerExamples
{
    public class AbrigoTemporarioCreateDtoExample : IExamplesProvider<AbrigoTemporarioCreateDto>
    {
        public AbrigoTemporarioCreateDto GetExamples()
        {
            return new AbrigoTemporarioCreateDto
            {
                Nome = "Abrigo Provisório Vila Aurora",
                Descricao = "Abrigo emergencial instalado no ginásio da escola municipal após as fortes chuvas. Oferece alimentação, colchões e espaço para pets.",
                CidadeMunicipio = "São Carlos",
                Bairro = "Vila Aurora",
                Logradouro = "Avenida Principal, 1234 - Ginásio Poliesportivo",
                Capacidade = 120,
                ImagemUrls = new List<string>
                {
                    "https://cdn.disasterlink.com.br/abrigos/vila_aurora_1.jpg",
                    "https://cdn.disasterlink.com.br/abrigos/vila_aurora_2.jpg"
                }
            };
        }
    }
}
