namespace APISales.Application.DTOs.Sales
{
    public class SaleItemResponseDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? ServiceItemId { get; set; }
        public int? ExecutorEmployeeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ItemStatus { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string? Notes { get; set; }
    }
}
