using APISales.Application.DTOs.Customers;
using APISales.Context;
using APISales.Domain.Customers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APISales.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CustomerController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> Get()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CustomerResponseDto>>(customers));
        }

        [HttpGet("{id:int}", Name = "GetCustomer")]
        public async Task<ActionResult<CustomerResponseDto>> Get(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound("Cliente nao encontrado!");

            return Ok(_mapper.Map<CustomerResponseDto>(customer));
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await PhoneAlreadyExists(dto.Phone))
                return Conflict("Ja existe um cliente cadastrado com este telefone.");

            var customer = _mapper.Map<Customer>(dto);

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var customerDto = _mapper.Map<CustomerResponseDto>(customer);

            return CreatedAtRoute("GetCustomer", new { id = customer.Id }, customerDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound("Cliente nao encontrado!");

            if (await PhoneAlreadyExists(dto.Phone, id))
                return Conflict("Ja existe um cliente cadastrado com este telefone.");

            _mapper.Map(dto, customer);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<CustomerResponseDto>(customer));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound("Cliente nao encontrado!");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<CustomerResponseDto>(customer));
        }

        private async Task<bool> PhoneAlreadyExists(string? phone, int? ignoredCustomerId = null)
        {
            var normalizedPhone = NormalizePhone(phone);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return false;

            var customers = await _context.Customers
                .Where(customer => ignoredCustomerId == null || customer.Id != ignoredCustomerId)
                .ToListAsync();

            return customers.Any(customer => NormalizePhone(customer.Phone) == normalizedPhone);
        }

        private static string NormalizePhone(string? phone)
        {
            return string.IsNullOrWhiteSpace(phone)
                ? string.Empty
                : new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}
