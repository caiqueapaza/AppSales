namespace APISales.Application.DTOs.User
{
    public class UserMeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string UserName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public bool IsAdmin { get; set; }
        public bool HasAppAccess { get; set; }
    }
}
