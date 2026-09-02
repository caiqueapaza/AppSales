using APISales.Application.DTOs.ServiceItens;
using APISales.Context;
using APISales.Domain.Sales;
using APISales.Domain.ServiceItens;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

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

        [HttpGet("suggestions")]
        public async Task<ActionResult<IEnumerable<ServiceSuggestionDto>>> GetSuggestions([FromQuery] ServicePriceSuggestionQueryDto query)
        {
            if (query.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == query.CategoryId.Value && c.IsActive))
                return NotFound("Categoria nao encontrada!");

            if (!string.IsNullOrWhiteSpace(query.AudienceType) && !SaleRepairOptions.IsValidAudienceType(query.AudienceType))
                return BadRequest("Publico invalido. Use: Adult ou Child.");

            if (!string.IsNullOrWhiteSpace(query.ActionType) && !SaleRepairOptions.IsValidActionType(query.ActionType))
                return BadRequest("Acao invalida. Use: Adjustment, Replacement, Addition ou Removal.");

            var audienceType = string.IsNullOrWhiteSpace(query.AudienceType)
                ? null
                : SaleRepairOptions.NormalizeAudienceType(query.AudienceType);
            var actionType = string.IsNullOrWhiteSpace(query.ActionType)
                ? null
                : SaleRepairOptions.NormalizeActionType(query.ActionType);
            var searchTerm = NormalizeSearchText(query.Search);
            var limit = Math.Clamp(query.Limit ?? 10, 1, 50);

            var activeServices = await _context.ServiceItens
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                activeServices = activeServices
                    .Where(service => MatchesServiceSearch(service, searchTerm))
                    .ToList();
            }

            var lastUsedByServiceId = await _context.SaleEntryItemServices
                .AsNoTracking()
                .GroupBy(service => service.ServiceItemId)
                .Select(group => new { ServiceItemId = group.Key, LastUsedAt = group.Max(service => service.CreatedAt) })
                .ToDictionaryAsync(item => item.ServiceItemId, item => (DateTime?)item.LastUsedAt);

            var orderedServices = activeServices
                .OrderByDescending(service => lastUsedByServiceId.ContainsKey(service.Id))
                .ThenByDescending(service => lastUsedByServiceId.TryGetValue(service.Id, out var lastUsedAt) ? lastUsedAt : null)
                .ThenBy(service => service.Name)
                .Take(limit)
                .ToList();

            var response = orderedServices.Select(service =>
            {
                var normalizedName = (service.Name ?? string.Empty).ToLowerInvariant();
                var priceSuggestion = BuildPriceSuggestion(service.Id, query.CategoryId, audienceType, actionType);
                lastUsedByServiceId.TryGetValue(service.Id, out var lastUsedAt);

                return new ServiceSuggestionDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Price = service.Price,
                    SuggestedPrice = priceSuggestion.Price,
                    HasHistoricalPrice = priceSuggestion.HasHistoricalPrice,
                    SuggestedPriceSource = priceSuggestion.Source,
                    RequiresHemCm = normalizedName.Contains("barra"),
                    RequiresCollarCm = normalizedName.Contains("gola"),
                    LastUsedAt = lastUsedAt,
                };
            });

            return Ok(response);
        }

        [HttpGet("suggestions/{categoryId:int}")]
        public Task<ActionResult<IEnumerable<ServiceSuggestionDto>>> GetSuggestionsByCategory(int categoryId, [FromQuery] ServicePriceSuggestionQueryDto query)
        {
            query.CategoryId = categoryId;
            return GetSuggestions(query);
        }

        [HttpGet("{id:int}", Name = "GetService")]
        public async Task<ActionResult<ServiceItemResponseDto>> Get(int id)
        {
            var service = await _context.ServiceItens.FindAsync(id);

            if (service is null)
                return NotFound("Servico nao encontrado!");

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }

        [HttpPost]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Post(CreateServiceItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = _mapper.Map<ServiceItem>(dto);
            service.ServiceType = string.IsNullOrWhiteSpace(service.ServiceType) ? string.Empty : service.ServiceType.Trim();

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
                return NotFound("Servico nao encontrado!");

            _mapper.Map(dto, service);
            service.ServiceType = string.IsNullOrWhiteSpace(service.ServiceType) ? string.Empty : service.ServiceType.Trim();

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var service = await _context.ServiceItens.FindAsync(id);
            if (service is null)
                return NotFound("Servico nao encontrado!");

            _context.ServiceItens.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ServiceItemResponseDto>(service));
        }

        private PriceSuggestion BuildPriceSuggestion(int serviceItemId, int? categoryId, string? audienceType, string? actionType)
        {
            IQueryable<SaleEntryItemService> query = _context.SaleEntryItemServices
                .AsNoTracking()
                .Include(service => service.SaleEntryItem)
                .Where(service => service.ServiceItemId == serviceItemId && service.UnitPrice > 0);

            var exact = TryAverage(query, categoryId, audienceType, actionType);
            if (exact.HasHistoricalPrice)
                return exact with { Source = "CategoryAudienceAction" };

            var categoryAndAction = TryAverage(query, categoryId, null, actionType);
            if (categoryAndAction.HasHistoricalPrice)
                return categoryAndAction with { Source = "CategoryAction" };

            var serviceAndAction = TryAverage(query, null, null, actionType);
            if (serviceAndAction.HasHistoricalPrice)
                return serviceAndAction with { Source = "Action" };

            var serviceOnly = TryAverage(query, null, null, null);
            if (serviceOnly.HasHistoricalPrice)
                return serviceOnly with { Source = "Service" };

            var defaultPrice = _context.ServiceItens
                .AsNoTracking()
                .Where(service => service.Id == serviceItemId)
                .Select(service => service.Price)
                .FirstOrDefault();
            return new PriceSuggestion(defaultPrice, false, "Default");
        }

        private static PriceSuggestion TryAverage(IQueryable<SaleEntryItemService> baseQuery, int? categoryId, string? audienceType, string? actionType)
        {
            var query = baseQuery;

            if (categoryId.HasValue)
                query = query.Where(service => service.SaleEntryItem != null && service.SaleEntryItem.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(audienceType))
                query = query.Where(service => service.SaleEntryItem != null && service.SaleEntryItem.AudienceType == audienceType);

            if (!string.IsNullOrWhiteSpace(actionType))
                query = query.Where(service => service.ActionType == actionType);

            var prices = query
                .OrderByDescending(service => service.CreatedAt)
                .Take(20)
                .Select(service => service.UnitPrice)
                .ToList();

            return prices.Count == 0
                ? new PriceSuggestion(0m, false, null)
                : new PriceSuggestion(Math.Round(prices.Average(), 2), true, null);
        }

        private static bool MatchesServiceSearch(ServiceItem service, string searchTerm)
        {
            var text = NormalizeSearchText(string.Join(" ", service.Name, service.Description, service.ServiceType));
            return text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSearchText(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed record PriceSuggestion(decimal Price, bool HasHistoricalPrice, string? Source);
    }
}
