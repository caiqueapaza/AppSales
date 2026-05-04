using APISales.Domain.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Employees
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcionario e obrigatorio!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "O e-mail informado e invalido!")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "O cargo do funcionario e obrigatorio!")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
