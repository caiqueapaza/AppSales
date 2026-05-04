using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.ServiceItens
{
    [Table("ServiceItens")]
    public class ServiceItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do serviço deve ser maior que zero!")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 
    }
}
