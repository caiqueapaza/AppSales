using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.Employees
{
    public class UpdateEmployeeDto
    {
        [Required(ErrorMessage = "O nome do funcionario e obrigatorio!")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail do funcionario e obrigatorio!")]
        [EmailAddress(ErrorMessage = "O e-mail informado e invalido!")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "O cargo do funcionario e obrigatorio!")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
