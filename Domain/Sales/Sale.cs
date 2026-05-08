using APISales.Domain.Customers;
using APISales.Domain.Employees;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Sales
{
    [Table("Sales")]
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O cliente da venda e obrigatorio!")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "O vendedor da venda e obrigatorio!")]
        public int SellerEmployeeId { get; set; }
        public Employee? SellerEmployee { get; set; }

        public DateTime DeliveryDate { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderStatus { get; set; } = "Received";

        [Required]
        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(30)]
        public string? PaymentMethod { get; set; }

        [StringLength(1000)]
        public string? ProblemDescription { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SubTotalAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime? CompletedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>(); // Produtos da venda
        public ICollection<SaleEntryItem> EntryItems { get; set; } = new List<SaleEntryItem>(); // Itens recebidos para reparo
        public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    }
}
