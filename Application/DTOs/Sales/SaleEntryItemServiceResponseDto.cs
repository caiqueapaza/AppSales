namespace APISales.Application.DTOs.Sales
{
    public class SaleEntryItemServiceResponseDto
    {
        public int Id { get; set; }
        public int ServiceItemId { get; set; }
        public string? ServiceItemName { get; set; }
        public int? ExecutorEmployeeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string MeasurementUnit { get; set; } = "uni";
        public string? RepairDescription { get; set; }
        public string ItemStatus { get; set; } = string.Empty;
        public DateTime? ReceivedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? DeliveredToName { get; set; }
        public int? DeliveredByEmployeeId { get; set; }
        public string? DeliveryNote { get; set; }
    }
}
