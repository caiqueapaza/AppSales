using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APISales.Domain.Products
{
    [Table("Categories")]
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatória!")]
        [StringLength(100)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }

    }
}
