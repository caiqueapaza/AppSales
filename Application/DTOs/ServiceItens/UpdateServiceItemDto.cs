namespace APISales.Application.DTOs.ServiceItens
{
    public class UpdateServiceItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
