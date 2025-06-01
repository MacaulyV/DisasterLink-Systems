using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DisasterLink_API.Utils
{
    public class CustomDateTimeJsonConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "dd/MM/yyyy HH:mm";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParseExact(reader.GetString(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
                // Tenta outros formatos comuns se o principal falhar, ou lança exceção
                // Por simplicidade, vamos assumir que o formato será sempre o esperado na leitura ou usar o padrão do .NET
                if (DateTime.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date))
                {
                    return date;
                }
            }
            // Fallback para o leitor padrão se não for string ou não puder ser parseado
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Se a data for MinValue, pode indicar que não foi definida (opcional)
            // if (value == DateTime.MinValue)
            // {
            //     writer.WriteNullValue();
            // }
            // else
            // {
            writer.WriteStringValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            // }
        }
    }
} 