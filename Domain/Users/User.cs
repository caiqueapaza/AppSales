using APISales.Domain.Employees;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APISales.Domain.Users
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O nome do usuário é obrigatório!")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O sobrenome do usuário é obrigatório!")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A senha do usuário é obrigatório!")]
        [StringLength(300)]
        public string? Password { get; set; }

        [StringLength(100)]
        public string? UserName { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
