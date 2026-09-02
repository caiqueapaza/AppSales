using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class CreateSaleEntryItemServiceDto
    {
        [Required]
        [RegularExpression("^(Adjustment|Replacement|Addition|Removal)$", ErrorMessage = "Acao invalida. Use: Adjustment, Replacement, Addition ou Removal.")]
        public string ActionType { get; set; } = "Adjustment";

        [Required]
        public int ServiceItemId { get; set; }

        public int? ExecutorEmployeeId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string? RepairDescription { get; set; }

        [Required]
        [RegularExpression("^(uni|cm|m)$", ErrorMessage = "Unidade de medida invalida. Use: uni, cm ou m.")]
        public string MeasurementUnit { get; set; } = "uni";
    }
}
