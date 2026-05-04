using APISales.Application.DTOs.ServiceItens;
using APISales.Context;
using APISales.Domain.ServiceItens;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APISales.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceItemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ServiceItemController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceItemResponseDto>>> Get()
        {
            var services = await _context.ServiceItens.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ServiceItemResponseDto>>(services));
        }

        [HttpGet("{id:int}", Name = "GetService")]
        public async Task<ActionResult<ServiceItemResponseDto>> Get(int id)
        {
            var service = await _context.ServiceItens.FindAsync(id);

            if (service is null)
                return NotFound("Serviço não encontrado!");

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Post(CreateServiceItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _mapper.Map<ServiceItem>(dto);

            _context.ServiceItens.Add(service);
            await _context.SaveChangesAsync();

            var serviceDto = _mapper.Map<ServiceItemResponseDto>(service);

            return CreatedAtRoute("GetService", new { id = service.Id }, serviceDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Put(int id, UpdateServiceItemDto dto)
        {
            var service = await _context.ServiceItens.FindAsync(id);

            if (service is null)
                return NotFound("Serviço não encontrado!");

            _mapper.Map(dto, service);

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var service = await _context.ServiceItens.FindAsync(id);
            if (service is null)
                return NotFound("Serviço não encontrado!");

            _context.ServiceItens.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }
    }
}
