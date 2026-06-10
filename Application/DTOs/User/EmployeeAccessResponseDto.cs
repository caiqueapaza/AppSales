namespace APISales.Application.DTOs.User
{
    public class EmployeeAccessResponseDto
    {
        public Guid UserId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool HasAppAccess { get; set; }
        public bool IsAdmin { get; set; }
    }
}
