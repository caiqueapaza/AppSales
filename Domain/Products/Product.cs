using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Products
{
    [Table("Products")]
    public class Product :IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório!")]
        [StringLength(200)]
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "A Categoria do produto é obrigatório!")]

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do produto deve ser maior que zero!")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Price > 10000 && string.IsNullOrEmpty(Description))
            {
                yield return new ValidationResult(
                    "Produtos acima de 10.000 precisam de descrição.",
                    new[] { nameof(Description) }
                );
            }
        }
    }
}
