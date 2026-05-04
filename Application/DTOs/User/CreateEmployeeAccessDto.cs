using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.User
{
    public class CreateEmployeeAccessDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;

        public bool HasAppAccess { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
    }
}
