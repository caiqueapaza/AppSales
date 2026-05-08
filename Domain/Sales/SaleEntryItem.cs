using APISales.Domain.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Sales
{
    [Table("SaleEntryItems")]
    public class SaleEntryItem
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ConditionNotes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<SaleEntryItemService> Services { get; set; } = new List<SaleEntryItemService>();
    }
}
