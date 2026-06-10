using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Customers
{
    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Cep { get; set; }
        public string? Address { get; set; }
        public string? District { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
