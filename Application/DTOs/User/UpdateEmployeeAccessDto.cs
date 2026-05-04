using System.ComponentModel.DataAnnotations;

namespace APISales.Application.DTOs.User
{
    public class UpdateEmployeeAccessDto
    {
        public bool HasAppAccess { get; set; }
        public bool IsAdmin { get; set; }

        [StringLength(100, MinimumLength = 4)]
        public string? Password { get; set; }
    }
}
