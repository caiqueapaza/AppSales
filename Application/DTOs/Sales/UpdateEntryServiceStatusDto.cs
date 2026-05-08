using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class UpdateEntryServiceStatusDto
    {
        [Required]
        [StringLength(30)]
        public string ItemStatus { get; set; } = "Received";

        [StringLength(150)]
        public string? DeliveredToName { get; set; }

        public int? DeliveredByEmployeeId { get; set; }

        [StringLength(500)]
        public string? DeliveryNote { get; set; }
    }
}
