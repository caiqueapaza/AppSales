using APISales.Domain.Sales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Customers
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do cliente e obrigatorio!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Document { get; set; }

        [EmailAddress(ErrorMessage = "O e-mail informado e invalido!")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string? Cep { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? District { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Sale>? Sales { get; set; }
    }
}
