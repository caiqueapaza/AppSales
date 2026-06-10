using APISales.Context;
using APISales.Domain.Users;
using APISales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using APISales.Infrastructure.Repositories;
using APISales.Application.DTOs.User;
using APISales.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace APISales.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;
        public UserController(IUserRepository repository, ILogger<UserController> logger, TokenService tokenService, AppDbContext context)
        {
            _repository = repository;
            _logger = logger;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "ADM")]
        public ActionResult<IEnumerable<User>> Get()
        {
            var users = _repository.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id:guid}", Name = "GetUser")]
        [Authorize(Roles = "ADM")]
        public ActionResult<User> Get(Guid id)
        {
            var user = _repository.GetUser(id);
            if (user == null)
            {
                _logger.LogWarning($"Usuário com id= {id} não encontrado!");
                return NotFound("Usuário com id= {id} não encontrado!");
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _repository.GetUser(Guid.Parse(userId));

            if (user == null)
                return NotFound();

            return Ok(new UserMeDto
            {
                Id = user.Id,
                Name = GetFullName(user),
                Email = user.Email,
                UserName = user.UserName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                IsAdmin = user.IsAdmin,
                HasAppAccess = user.IsActive
            });
        }

        [HttpPut("me")]
        public IActionResult PutMe(UpdateUserProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _repository.GetUser(Guid.Parse(userId));

            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("O nome completo é obrigatório.");

            var nameParts = dto.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.Name = nameParts[0];
            user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            user.Email = dto.Email?.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            _repository.Update(user);

            return Ok(new UserMeDto
            {
                Id = user.Id,
                Name = GetFullName(user),
                Email = user.Email,
                UserName = user.UserName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                IsAdmin = user.IsAdmin,
                HasAppAccess = user.IsActive
            });
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public ActionResult Post(User user)
        {
            if (user == null)
            {
                _logger.LogWarning($"Dados invalidos!");
                return BadRequest($"Dados invalidos!");
            }
            var userCreated = _repository.Create(user);

            return new CreatedAtRouteResult("GetUser", new { id = userCreated.Id }, userCreated);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginModel login)
        {
            var user = _repository.GetLogin(login.UserName, login.Password);

            if (user == null)
                return Unauthorized("Usuário ou senha inválidos!");

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                userName = user.UserName,
                isAdmin = user.IsAdmin,
                hasAppAccess = user.IsActive,
                token = token,
            });
        }

        [HttpGet("employee-access")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult<IEnumerable<EmployeeAccessResponseDto>>> GetEmployeeAccess([FromQuery] string? search = null)
        {
            var query = _context.Users
                .Include(u => u.Employee)
                .Where(u => u.EmployeeId != null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.Employee != null && u.Employee.Name.ToLower().Contains(term)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(term)));
            }

            var users = await query
                .OrderByDescending(u => u.UpdatedAt)
                .ToListAsync();

            return Ok(users.Select(ToEmployeeAccessResponse));
        }

        [HttpGet("employee-access/{employeeId:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult<EmployeeAccessResponseDto>> GetEmployeeAccessByEmployee(int employeeId)
        {
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            if (user == null)
                return NotFound("Acesso do funcionário não encontrado.");

            return Ok(ToEmployeeAccessResponse(user));
        }

        [HttpPost("employee-access")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult<EmployeeAccessResponseDto>> CreateEmployeeAccess(CreateEmployeeAccessDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee == null)
                return NotFound("Funcionário não encontrado.");

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == dto.EmployeeId);
            if (existing != null)
                return Conflict("Este funcionário já possui acesso ao app.");

            var nameParts = SplitName(employee.Name);
            var user = new User
            {
                Name = nameParts.firstName,
                LastName = nameParts.lastName,
                Email = employee.Email,
                Password = dto.Password,
                EmployeeId = employee.Id,
                IsActive = dto.HasAppAccess,
                IsAdmin = dto.IsAdmin,
            };

            var created = _repository.Create(user);
            var createdWithEmployee = await _context.Users.Include(u => u.Employee).FirstAsync(u => u.Id == created.Id);

            return CreatedAtAction(nameof(GetEmployeeAccessByEmployee), new { employeeId = employee.Id }, ToEmployeeAccessResponse(createdWithEmployee));
        }

        [HttpPut("employee-access/{employeeId:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult<EmployeeAccessResponseDto>> UpdateEmployeeAccess(int employeeId, UpdateEmployeeAccessDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            if (user == null)
                return NotFound("Acesso do funcionário não encontrado.");

            user.IsActive = dto.HasAppAccess;
            user.IsAdmin = dto.IsAdmin;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            _repository.Update(user);
            var updated = await _context.Users.Include(u => u.Employee).FirstAsync(u => u.Id == user.Id);
            return Ok(ToEmployeeAccessResponse(updated));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "ADM")]
        public ActionResult Put(Guid id, User user)
        {
            if (id != user.Id)
            {
                _logger.LogWarning($"Dados invalidos!");
                return BadRequest($"Dados invalidos!");
            }
            _repository.Update(user);

            return Ok(user);
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "ADM")]
        public ActionResult Delete(Guid id)
        {
            var user = _repository.GetUser(id);
            if (user == null)
            {
                _logger.LogWarning($"Usuário com id= {id} não encontrado!");
                return NotFound("Usuário com id= {id} não encontrado!");
            }

            var userDeleted = _repository.Delete(id);
            return Ok(userDeleted);
        }

        private static string GetFullName(User user)
        {
            return string.Join(" ", new[] { user.Name, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static (string firstName, string lastName) SplitName(string fullName)
        {
            var parts = (fullName ?? string.Empty).Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return ("usuario", "sistema");
            if (parts.Length == 1)
                return (parts[0], "usuario");
            return (parts[0], parts[1]);
        }

        private static EmployeeAccessResponseDto ToEmployeeAccessResponse(User user)
        {
            return new EmployeeAccessResponseDto
            {
                UserId = user.Id,
                EmployeeId = user.EmployeeId ?? 0,
                EmployeeName = user.Employee?.Name ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                HasAppAccess = user.IsActive,
                IsAdmin = user.IsAdmin
            };
        }
    }
}
