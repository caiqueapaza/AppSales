using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Customers
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "O nome do cliente e obrigatorio!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Document { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "O telefone do cliente e obrigatorio!")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Cep { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? District { get; set; }
    }
}
