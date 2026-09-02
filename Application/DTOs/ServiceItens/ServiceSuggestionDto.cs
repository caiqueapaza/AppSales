namespace APISales.Application.DTOs.ServiceItens
{
    public class ServiceSuggestionDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal SuggestedPrice { get; set; }
        public bool HasHistoricalPrice { get; set; }
        public string? SuggestedPriceSource { get; set; }
        public bool RequiresHemCm { get; set; }
        public bool RequiresCollarCm { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
