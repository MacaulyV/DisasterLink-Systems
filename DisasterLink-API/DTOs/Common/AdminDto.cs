using System;
using System.Text.Json.Serialization;

namespace DisasterLink_API.DTOs.Admin
{
    public class AdminDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Propriedade para AutoMapper e uso interno, não serializada diretamente no JSON.
        [JsonIgnore]
        public DateTime DataCriacaoValue { get; set; } // Esta armazena o valor DateTime UTC

        /// <summary>
        /// Data de criação do administrador (formato: dd/MM/yyyy HH:mm, Horário de Brasília).
        /// Esta propriedade será serializada como "data" no JSON.
        /// </summary>
        public string Data 
        {
            get 
            {
                try
                {
                    TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); // Ou "Brazil/East" em sistemas Linux/macOS
                    DateTimeOffset brazilTime = TimeZoneInfo.ConvertTimeFromUtc(DataCriacaoValue, brazilTimeZone);
                    return brazilTime.ToString("dd/MM/yyyy HH:mm");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback se o fuso horário não for encontrado (pode acontecer em alguns sistemas)
                    // Nesse caso, formata como UTC e adiciona um aviso
                    return DataCriacaoValue.ToString("dd/MM/yyyy HH:mm") + " (UTC)"; 
                }
                catch (Exception)
                {
                    // Fallback genérico, retorna UTC
                    return DataCriacaoValue.ToString("dd/MM/yyyy HH:mm") + " (UTC - Erro na conversão)";
                }
            }
        }
    }
} 