namespace APISales.Application.DTOs.Sales
{
    public class SaleEntryItemResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ConditionNotes { get; set; }
        public string AudienceType { get; set; } = string.Empty;
        public List<SaleEntryItemServiceResponseDto> Services { get; set; } = new();
    }
}
