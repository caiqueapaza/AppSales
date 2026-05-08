using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class CreateSalePaymentDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string Method { get; set; } = "Pix";

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
