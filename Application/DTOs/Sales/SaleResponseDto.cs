using APISales.Application.DTOs.Customers;

namespace APISales.Application.DTOs.Sales
{
    public class SaleResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int SellerEmployeeId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? ProblemDescription { get; set; }
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public CustomerResponseDto? Customer { get; set; }
        public List<SaleItemResponseDto> Items { get; set; } = new();
        public List<SaleEntryItemResponseDto> EntryItems { get; set; } = new();
        public List<SalePaymentResponseDto> Payments { get; set; } = new();
    }
}
