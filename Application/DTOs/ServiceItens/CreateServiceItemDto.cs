namespace APISales.Application.DTOs.ServiceItens
{
    public class CreateServiceItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ServiceType { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
