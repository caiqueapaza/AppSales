using APISales.Domain.Employees;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Sales
{
    [Table("SalePayments")]
    public class SalePayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string Method { get; set; } = "Pix";

        [StringLength(500)]
        public string? Note { get; set; }

        public int? ReceivedByEmployeeId { get; set; }
        public Employee? ReceivedByEmployee { get; set; }

        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
