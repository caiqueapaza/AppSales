namespace APISales.Application.DTOs.ServiceItens
{
    public class CreateServiceItemDto
    {
        public string Name { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
