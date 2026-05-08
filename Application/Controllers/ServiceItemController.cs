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

        [HttpGet("suggestions/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<ServiceSuggestionDto>>> GetSuggestionsByCategory(int categoryId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);
            if (category is null)
                return NotFound("Categoria não encontrada!");

            var normalizedCategory = Normalize(category.Name);
            var activeServices = await _context.ServiceItens
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Regras novas:
            // - cada serviço fica vinculado a uma categoria via ServiceType
            // - mesmo nome pode existir em categorias diferentes com preços diferentes
            var selectedServices = activeServices
                .Where(service => Normalize(service.ServiceType) == normalizedCategory)
                .ToList();

            var response = selectedServices.Select(service =>
            {
                var normalizedName = (service.Name ?? string.Empty).ToLowerInvariant();
                return new ServiceSuggestionDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Price = service.Price,
                    RequiresHemCm = normalizedName.Contains("barra"),
                    RequiresCollarCm = normalizedName.Contains("gola"),
                };
            });

            return Ok(response);
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
            service.ServiceType = string.IsNullOrWhiteSpace(service.ServiceType) ? "Geral" : service.ServiceType.Trim();

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
            service.ServiceType = string.IsNullOrWhiteSpace(service.ServiceType) ? "Geral" : service.ServiceType.Trim();

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

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
