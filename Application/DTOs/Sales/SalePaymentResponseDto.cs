namespace APISales.Application.DTOs.Sales
{
    public class SalePaymentResponseDto
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int? ReceivedByEmployeeId { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
