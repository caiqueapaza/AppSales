using APISales.Domain.Employees;
using APISales.Domain.ServiceItens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Sales
{
    [Table("SaleEntryItemServices")]
    public class SaleEntryItemService
    {
        [Key]
        public int Id { get; set; }

        public int SaleEntryItemId { get; set; }
        public SaleEntryItem? SaleEntryItem { get; set; }

        public int ServiceItemId { get; set; }
        public ServiceItem? ServiceItem { get; set; }

        public int? ExecutorEmployeeId { get; set; }
        public Employee? ExecutorEmployee { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(12,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string? RepairDescription { get; set; }

        [Required]
        [StringLength(20)]
        public string ActionType { get; set; } = "Adjustment";

        [Required]
        [StringLength(10)]
        public string MeasurementUnit { get; set; } = "uni";

        [StringLength(30)]
        public string ItemStatus { get; set; } = "Received";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? ReceivedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CanceledAt { get; set; }

        [StringLength(150)]
        public string? DeliveredToName { get; set; }

        public int? DeliveredByEmployeeId { get; set; }
        public Employee? DeliveredByEmployee { get; set; }

        [StringLength(500)]
        public string? DeliveryNote { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
