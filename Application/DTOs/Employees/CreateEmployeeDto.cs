using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Employees
{
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "O nome do funcionario e obrigatorio!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "O e-mail informado e invalido!")]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }
    }
}
