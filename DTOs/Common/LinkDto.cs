namespace DisasterLink_API.DTOs
{
    /// <summary>
    /// Representa um link HATEOAS
    /// </summary>
    public class LinkDto
    {
        public string Href { get; set; } = null!;
        public string Rel { get; set; } = null!;
        public string Method { get; set; } = null!;
    }
} 