namespace APISales.Application.DTOs.ServiceItens
{
    public class ServiceSuggestionDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool RequiresHemCm { get; set; }
        public bool RequiresCollarCm { get; set; }
    }
}
