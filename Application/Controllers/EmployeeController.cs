using APISales.Application.DTOs.Employees;
using APISales.Context;
using APISales.Domain.Employees;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APISales.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> Get()
        {
            var employees = await _context.Employees.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<EmployeeResponseDto>>(employees));
        }

        [HttpGet("{id:int}", Name = "GetEmployee")]
        public async Task<ActionResult<EmployeeResponseDto>> Get(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee is null)
                return NotFound("Funcionario nao encontrado!");

            return Ok(_mapper.Map<EmployeeResponseDto>(employee));
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Post(CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = _mapper.Map<Employee>(dto);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var employeeDto = _mapper.Map<EmployeeResponseDto>(employee);

            return CreatedAtRoute("GetEmployee", new { id = employee.Id }, employeeDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Put(int id, UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.Employees.FindAsync(id);

            if (employee is null)
                return NotFound("Funcionario nao encontrado!");

            _mapper.Map(dto, employee);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<EmployeeResponseDto>(employee));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee is null)
                return NotFound("Funcionario nao encontrado!");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<EmployeeResponseDto>(employee));
        }
    }
}
