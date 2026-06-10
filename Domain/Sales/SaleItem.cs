using APISales.Domain.Employees;
using APISales.Domain.Products;
using APISales.Domain.ServiceItens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Sales
{
    [Table("SaleItens")]
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ServiceItemId { get; set; }
        public ServiceItem? ServiceItem { get; set; }

        public int? ExecutorEmployeeId { get; set; }
        public Employee? ExecutorEmployee { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero!")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitario deve ser maior que zero!")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(30)]
        public string ItemStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? ItemDescription { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
