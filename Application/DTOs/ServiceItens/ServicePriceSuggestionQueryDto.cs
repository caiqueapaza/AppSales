namespace APISales.Application.DTOs.ServiceItens
{
    public class ServicePriceSuggestionQueryDto
    {
        public int? CategoryId { get; set; }
        public string? AudienceType { get; set; }
        public string? ActionType { get; set; }
        public string? Search { get; set; }
        public int? Limit { get; set; }
    }
}
