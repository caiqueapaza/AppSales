using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Sales
{
    public class CreateSaleDto
    {
        [Required(ErrorMessage = "O cliente da venda e obrigatorio!")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "O vendedor da venda e obrigatorio!")]
        public int SellerEmployeeId { get; set; }

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

        // Produtos da venda (podem existir sem reparo)
        public List<CreateSaleItemDto> Items { get; set; } = new();

        // Itens recebidos para reparo, cada um com seus serviços
        public List<CreateSaleEntryItemDto> EntryItems { get; set; } = new();
    }
}
