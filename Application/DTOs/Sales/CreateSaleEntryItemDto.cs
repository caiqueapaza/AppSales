using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class CreateSaleEntryItemDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ConditionNotes { get; set; }

        [Required]
        [RegularExpression("^(Adult|Child)$", ErrorMessage = "Publico invalido. Use: Adult ou Child.")]
        public string AudienceType { get; set; } = "Adult";

        [MinLength(1, ErrorMessage = "Cada item de entrada precisa ter pelo menos um serviço.")]
        public List<CreateSaleEntryItemServiceDto> Services { get; set; } = new();
    }
}
