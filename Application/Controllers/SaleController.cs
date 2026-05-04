using APISales.Application.DTOs.Sales;
using APISales.Context;
using APISales.Domain.Sales;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APISales.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SaleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SaleController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleResponseDto>>> Get()
        {
            var sales = await _context.Sales
                .Include(s => s.Items)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SaleResponseDto>>(sales));
        }

        [HttpGet("{id:int}", Name = "GetSale")]
        public async Task<ActionResult<SaleResponseDto>> Get(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationError = await ValidateSale(dto);

            if (validationError is not null)
                return BadRequest(validationError);

            var subTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

            var sale = new Sale
            {
                CustomerId = dto.CustomerId,
                SellerEmployeeId = dto.SellerEmployeeId,
                DeliveryDate = dto.DeliveryDate,
                OrderStatus = dto.OrderStatus,
                PaymentStatus = dto.PaymentStatus,
                PaymentMethod = dto.PaymentMethod,
                ProblemDescription = dto.ProblemDescription,
                Notes = dto.Notes,
                SubTotalAmount = subTotal,
                DiscountAmount = dto.DiscountAmount,
                TotalAmount = subTotal - dto.DiscountAmount,
                AmountPaid = dto.AmountPaid,
                Items = _mapper.Map<List<SaleItem>>(dto.Items)
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            var saleDto = _mapper.Map<SaleResponseDto>(sale);

            return CreatedAtRoute("GetSale", new { id = sale.Id }, saleDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, UpdateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            var isCancelTransition = sale.OrderStatus != "Canceled" && dto.OrderStatus == "Canceled";
            var isAdmin = User.IsInRole("ADM") || User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADM");
            if (isCancelTransition && !isAdmin)
                return Forbid();

            var subTotal = sale.Items.Sum(i => i.Quantity * i.UnitPrice);

            if (dto.DiscountAmount > subTotal)
                return BadRequest("O desconto nao pode ser maior que o valor total dos itens.");

            if (dto.AmountPaid > subTotal - dto.DiscountAmount)
                return BadRequest("O valor pago nao pode ser maior que o total final da venda.");

            sale.DeliveryDate = dto.DeliveryDate;
            sale.OrderStatus = dto.OrderStatus;
            sale.PaymentStatus = dto.PaymentStatus;
            sale.PaymentMethod = dto.PaymentMethod;
            sale.ProblemDescription = dto.ProblemDescription;
            sale.Notes = dto.Notes;
            sale.SubTotalAmount = subTotal;
            sale.DiscountAmount = dto.DiscountAmount;
            sale.TotalAmount = subTotal - dto.DiscountAmount;
            sale.AmountPaid = dto.AmountPaid;
            sale.UpdatedAt = DateTime.UtcNow;
            sale.CompletedAt = dto.OrderStatus == "Ready" ? sale.CompletedAt ?? DateTime.UtcNow : sale.CompletedAt;
            sale.DeliveredAt = dto.OrderStatus == "Delivered" ? sale.DeliveredAt ?? DateTime.UtcNow : sale.DeliveredAt;

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADM")]
        public async Task<ActionResult> Delete(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        private async Task<string?> ValidateSale(CreateSaleDto dto)
        {
            if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
                return "Cliente nao encontrado!";

            if (!await _context.Employees.AnyAsync(e => e.Id == dto.SellerEmployeeId))
                return "Vendedor nao encontrado!";

            if (dto.Items.Count == 0)
                return "A venda precisa ter pelo menos um item!";

            var subTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

            if (dto.DiscountAmount > subTotal)
                return "O desconto nao pode ser maior que o valor total dos itens.";

            if (dto.AmountPaid > subTotal - dto.DiscountAmount)
                return "O valor pago nao pode ser maior que o total final da venda.";

            foreach (var item in dto.Items)
            {
                var hasProduct = item.ProductId.HasValue;
                var hasService = item.ServiceItemId.HasValue;

                if (hasProduct == hasService)
                    return "Cada item precisa informar ProductId ou ServiceItemId, mas nao ambos.";

                if (hasProduct && !await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                    return $"Produto {item.ProductId} nao encontrado!";

                if (hasService && !await _context.ServiceItens.AnyAsync(s => s.Id == item.ServiceItemId))
                    return $"Servico {item.ServiceItemId} nao encontrado!";

                if (item.ExecutorEmployeeId.HasValue && !await _context.Employees.AnyAsync(e => e.Id == item.ExecutorEmployeeId))
                    return $"Executor {item.ExecutorEmployeeId} nao encontrado!";
            }

            return null;
        }
    }
}
