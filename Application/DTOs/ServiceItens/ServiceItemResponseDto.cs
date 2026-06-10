namespace APISales.Application.DTOs.ServiceItens
{
    public class ServiceItemResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
