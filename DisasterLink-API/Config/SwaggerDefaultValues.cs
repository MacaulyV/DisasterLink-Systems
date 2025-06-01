using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DisasterLink_API.Config
{
    /// <summary>
    /// Classe para configurar os valores padrão no Swagger
    /// </summary>
    public class SwaggerDefaultValues : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null || context.Type == null)
                return;

            var properties = context.Type.GetProperties()
                .Where(p => p.GetCustomAttribute<DefaultValueAttribute>() != null);

            foreach (var property in properties)
            {
                var defaultAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultAttribute?.Value == null) continue;

                var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                if (schema.Properties.TryGetValue(propertyName, out var openApiProperty))
                {
                    var defaultValue = defaultAttribute.Value;
                    
                    if (defaultValue is string stringValue)
                        openApiProperty.Example = new OpenApiString(stringValue);
                    else if (defaultValue is int intValue)
                        openApiProperty.Example = new OpenApiInteger(intValue);
                    else if (defaultValue is long longValue)
                        openApiProperty.Example = new OpenApiLong(longValue);
                    else if (defaultValue is double doubleValue)
                        openApiProperty.Example = new OpenApiDouble(doubleValue);
                    else if (defaultValue is float floatValue)
                        openApiProperty.Example = new OpenApiFloat(floatValue);
                    else if (defaultValue is bool boolValue)
                        openApiProperty.Example = new OpenApiBoolean(boolValue);
                    else if (defaultValue is DateTime dateTimeValue)
                        openApiProperty.Example = new OpenApiDateTime(dateTimeValue);
                }
            }
        }
    }
} 