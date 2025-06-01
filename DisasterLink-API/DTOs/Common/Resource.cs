using System.Collections.Generic;

namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// Representa um recurso enriquecido com links HATEOAS
    /// </summary>
    public class Resource<T>
    {
        public T Value { get; set; } = default!;
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
} 