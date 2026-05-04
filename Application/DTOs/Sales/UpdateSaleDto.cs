using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class UpdateSaleDto
    {
        public DateTime DeliveryDate { get; set; }

        [StringLength(30)]
        public string OrderStatus { get; set; } = "Received";

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(30)]
        public string? PaymentMethod { get; set; }

        [StringLength(1000)]
        public string? ProblemDescription { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O desconto nao pode ser negativo!")]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O valor pago nao pode ser negativo!")]
        public decimal AmountPaid { get; set; }
    }
}
