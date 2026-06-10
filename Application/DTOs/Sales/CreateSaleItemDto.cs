using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class CreateSaleItemDto
    {
        public int? ProductId { get; set; }
        public int? ServiceItemId { get; set; }
        public int? ExecutorEmployeeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero!")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor unitario deve ser maior que zero!")]
        public decimal UnitPrice { get; set; }

        [StringLength(30)]
        public string ItemStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? ItemDescription { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
