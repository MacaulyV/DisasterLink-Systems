using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace DisasterLink_API.Config
{
    /// <summary>
    /// Ordena as tags do Swagger em ordem alfabética
    /// </summary>
    public class SwaggerTagsSorter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags != null)
            {
                swaggerDoc.Tags = swaggerDoc.Tags
                    .OrderBy(t => t.Name)
                    .ToList();
            }
        }
    }
} 