using APISales.Application.DTOs.Sales;
using APISales.Application.Services;
using APISales.Context;
using APISales.Domain.Sales;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SaleController(AppDbContext context, IMapper mapper, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleResponseDto>>> Get()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .Include(s => s.Payments)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<SaleResponseDto>>(sales));
        }

        [HttpGet("{id:int}", Name = "GetSale")]
        public async Task<ActionResult<SaleResponseDto>> Get(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .Include(s => s.Payments)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        [HttpGet("{id:int}/receipt-pdf")]
        public async Task<ActionResult> GetReceiptPdf(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            var storeName = _configuration["Store:Name"];
            var receiptTitle = _configuration["Store:ReceiptTitle"];
            var storeAddress = _configuration["Store:Address"];
            var storePhone = _configuration["Store:Phone"];
            var storeEmail = _configuration["Store:Email"];
            var options = new ReceiptPdfOptions
            {
                StoreName = storeName ?? "KuwenSys - Loja de Consertos",
                ReceiptTitle = receiptTitle ?? "Ordem de Servico",
                StoreAddress = storeAddress ?? string.Empty,
                StorePhone = storePhone ?? string.Empty,
                StoreEmail = storeEmail ?? string.Empty,
                StoreLogoPath = ResolveAssetPath(_configuration["Store:LogoPath"]),
                AppLogoPath = ResolveAssetPath(_configuration["Store:AppLogoPath"] ?? "assets/logo-app.png"),
                PickupDeadlineDays = Math.Max(_configuration.GetValue<int?>("Store:PickupDeadlineDays") ?? 30, 1),
                AdjustmentDeadlineDays = Math.Max(_configuration.GetValue<int?>("Store:AdjustmentDeadlineDays") ?? 7, 1),
                PickupPolicyText = _configuration["Store:PickupPolicyText"] ?? string.Empty,
                AdjustmentPolicyText = _configuration["Store:AdjustmentPolicyText"] ?? string.Empty,
            };
            var bytes = ProfessionalReceiptPdfBuilder.Build(sale, options);
            var customerName = sale.Customer?.Name ?? $"cliente-{sale.CustomerId}";
            var fileName = $"{SanitizeFileName(storeName ?? "loja")}-{SanitizeFileName(customerName)}-{sale.Id}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationError = await ValidateSale(dto);

            if (validationError is not null)
                return BadRequest(validationError);

            var productsSubTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
            var servicesSubTotal = dto.EntryItems.Sum(ei => ei.Services.Sum(s => s.Quantity * s.UnitPrice));
            var subTotal = productsSubTotal + servicesSubTotal;

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
                Items = _mapper.Map<List<SaleItem>>(dto.Items),
                EntryItems = _mapper.Map<List<SaleEntryItem>>(dto.EntryItems)
            };

            foreach (var entryItem in sale.EntryItems)
            {
                foreach (var service in entryItem.Services)
                {
                    service.ItemStatus = "Received";
                    service.ReceivedAt ??= DateTime.UtcNow;
                }
            }

            if (sale.AmountPaid > 0)
            {
                sale.Payments.Add(new SalePayment
                {
                    Amount = sale.AmountPaid,
                    Method = string.IsNullOrWhiteSpace(sale.PaymentMethod) ? "Pix" : sale.PaymentMethod.Trim(),
                    Note = "Pagamento inicial no cadastro da ordem.",
                    ReceivedByEmployeeId = TryReadEmployeeIdFromClaims(),
                    PaidAt = DateTime.UtcNow,
                });
            }

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
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .Include(s => s.Payments)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            var isCancelTransition = sale.OrderStatus != "Canceled" && dto.OrderStatus == "Canceled";
            var isAdmin = User.IsInRole("ADM") || User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "ADM");
            if (isCancelTransition && !isAdmin)
                return Forbid();

            var productsSubTotal = sale.Items.Sum(i => i.Quantity * i.UnitPrice);
            var servicesSubTotal = sale.EntryItems.Sum(ei => ei.Services.Sum(s => s.Quantity * s.UnitPrice));
            var subTotal = productsSubTotal + servicesSubTotal;

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
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .Include(s => s.Payments)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        [HttpPut("entry-service/{id:int}/status")]
        public async Task<ActionResult> UpdateEntryServiceStatus(int id, UpdateEntryServiceStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = await _context.SaleEntryItemServices
                .Include(s => s.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null)
                return NotFound("Serviço do item não encontrado!");

            var allowed = new[] { "Received", "InRepair", "Ready", "Delivered", "Canceled" };
            if (!allowed.Contains(dto.ItemStatus))
                return BadRequest("Status inválido para o serviço.");

            if (dto.ItemStatus == "Delivered")
            {
                if (string.IsNullOrWhiteSpace(dto.DeliveredToName))
                    return BadRequest("Informe para quem o item foi entregue.");

                if (!dto.DeliveredByEmployeeId.HasValue)
                    return BadRequest("Informe quem realizou a entrega.");

                if (!await _context.Employees.AnyAsync(e => e.Id == dto.DeliveredByEmployeeId.Value))
                    return BadRequest("Funcionário de entrega não encontrado.");
            }

            ApplyServiceStatus(service, dto);
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<SaleEntryItemServiceResponseDto>(service));
        }

        [HttpPost("{id:int}/payments")]
        public async Task<ActionResult<SaleResponseDto>> AddPayment(int id, CreateSalePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .Include(s => s.Payments)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Category)
                .Include(s => s.EntryItems)
                    .ThenInclude(ei => ei.Services)
                        .ThenInclude(es => es.ServiceItem)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                return NotFound("Venda nao encontrada!");

            if (sale.Payments.Count == 0 && sale.AmountPaid > 0)
            {
                sale.Payments.Add(new SalePayment
                {
                    Amount = sale.AmountPaid,
                    Method = string.IsNullOrWhiteSpace(sale.PaymentMethod) ? dto.Method.Trim() : sale.PaymentMethod.Trim(),
                    Note = "Saldo inicial registrado antes do historico de pagamentos.",
                    ReceivedByEmployeeId = TryReadEmployeeIdFromClaims(),
                    PaidAt = DateTime.UtcNow,
                });
            }

            sale.Payments.Add(new SalePayment
            {
                Amount = dto.Amount,
                Method = dto.Method.Trim(),
                Note = dto.Note?.Trim(),
                ReceivedByEmployeeId = TryReadEmployeeIdFromClaims(),
                PaidAt = DateTime.UtcNow,
            });

            RecalculatePaymentSummaryFromHistory(sale);
            sale.PaymentMethod = dto.Method.Trim();

            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<SaleResponseDto>(sale));
        }

        private async Task<string?> ValidateSale(CreateSaleDto dto)
        {
            if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
                return "Cliente nao encontrado!";

            if (!await _context.Employees.AnyAsync(e => e.Id == dto.SellerEmployeeId))
                return "Vendedor nao encontrado!";

            var hasProducts = dto.Items.Count > 0;
            var hasEntryItems = dto.EntryItems.Count > 0;

            if (!hasProducts && !hasEntryItems)
                return "A venda precisa ter pelo menos um produto ou item para reparo!";

            var subTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice)
                + dto.EntryItems.Sum(ei => ei.Services.Sum(s => s.Quantity * s.UnitPrice));

            if (dto.DiscountAmount > subTotal)
                return "O desconto nao pode ser maior que o valor total dos itens.";

            if (dto.AmountPaid > subTotal - dto.DiscountAmount)
                return "O valor pago nao pode ser maior que o total final da venda.";

            foreach (var item in dto.Items)
            {
                var hasProduct = item.ProductId.HasValue;
                var hasService = item.ServiceItemId.HasValue;

                if (!hasProduct || hasService)
                    return "Itens de venda devem informar apenas ProductId.";

                if (hasProduct && !await _context.Products.AnyAsync(p => p.Id == item.ProductId))
                    return $"Produto {item.ProductId} nao encontrado!";

                if (item.ExecutorEmployeeId.HasValue)
                    return "Itens de venda nao devem informar executor.";
            }

            foreach (var entryItem in dto.EntryItems)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == entryItem.CategoryId))
                    return $"Categoria {entryItem.CategoryId} nao encontrada!";

                if (entryItem.Services.Count == 0)
                    return "Cada item de entrada precisa ter pelo menos um serviço.";

                foreach (var service in entryItem.Services)
                {
                    if (!await _context.ServiceItens.AnyAsync(s => s.Id == service.ServiceItemId))
                        return $"Servico {service.ServiceItemId} nao encontrado!";

                    if (service.ExecutorEmployeeId.HasValue && !await _context.Employees.AnyAsync(e => e.Id == service.ExecutorEmployeeId))
                        return $"Executor {service.ExecutorEmployeeId} nao encontrado!";

                    var unit = (service.MeasurementUnit ?? "uni").Trim().ToLowerInvariant();
                    if (unit != "uni" && unit != "cm" && unit != "m")
                        return $"Unidade de medida invalida para o servico {service.ServiceItemId}. Use uni, cm ou m.";

                    service.MeasurementUnit = unit;
                }
            }

            return null;
        }

        private static void ApplyServiceStatus(SaleEntryItemService service, UpdateEntryServiceStatusDto dto)
        {
            service.ItemStatus = dto.ItemStatus;

            if (dto.ItemStatus == "Received")
            {
                service.ReceivedAt ??= DateTime.UtcNow;
            }
            else if (dto.ItemStatus == "InRepair")
            {
                service.StartedAt ??= DateTime.UtcNow;
            }
            else if (dto.ItemStatus == "Ready")
            {
                service.ReadyAt ??= DateTime.UtcNow;
            }
            else if (dto.ItemStatus == "Delivered")
            {
                service.DeliveredAt ??= DateTime.UtcNow;
                service.DeliveredToName = dto.DeliveredToName?.Trim();
                service.DeliveredByEmployeeId = dto.DeliveredByEmployeeId;
                service.DeliveryNote = dto.DeliveryNote?.Trim();
            }
            else if (dto.ItemStatus == "Canceled")
            {
                service.CanceledAt ??= DateTime.UtcNow;
            }
        }

        private static void RecalculatePaymentSummaryFromHistory(Sale sale)
        {
            var paid = sale.Payments.Sum(p => p.Amount);
            sale.AmountPaid = paid;

            if (sale.TotalAmount <= 0 || paid >= sale.TotalAmount)
                sale.PaymentStatus = "Paid";
            else if (paid > 0)
                sale.PaymentStatus = "Partial";
            else
                sale.PaymentStatus = "Pending";
        }

        private int? TryReadEmployeeIdFromClaims()
        {
            var raw = User.Claims.FirstOrDefault(c => c.Type == "employeeId")?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type == "employee_id")?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;

            return int.TryParse(raw, out var employeeId) ? employeeId : null;
        }

        private static List<string> BuildReceiptLines(
            Sale sale,
            string? storeName,
            string? receiptTitle,
            string? storeAddress,
            string? storePhone,
            string? storeEmail,
            int? pickupDeadlineDays,
            int? adjustmentDeadlineDays,
            string? pickupPolicyText,
            string? adjustmentPolicyText)
        {
            var culture = new CultureInfo("pt-BR");
            var customerName = sale.Customer?.Name?.Trim();
            var customerPhone = sale.Customer?.Phone?.Trim();
            var storeHeader = !string.IsNullOrWhiteSpace(storeName) ? storeName.Trim() : "KuwenSys - Loja de Consertos";
            var titleHeader = !string.IsNullOrWhiteSpace(receiptTitle) ? receiptTitle.Trim() : "Ordem de Servico";
            var deliveredRecipients = sale.EntryItems?
                .SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>())
                .Where(s => string.Equals(s.ItemStatus, "Delivered", StringComparison.OrdinalIgnoreCase))
                .Select(s => (s.DeliveredToName ?? string.Empty).Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
            var receiptStatusLine = BuildReceiptStatusLine(sale);

            var lines = new List<string>
            {
                storeHeader.ToUpperInvariant(),
                titleHeader,
                BuildStoreContactsLine(storeAddress, storePhone, storeEmail),
                "------------------------------------------------------------",
                string.Empty,
                $"Ordem #{sale.Id}",
                $"Cliente: {(!string.IsNullOrWhiteSpace(customerName) ? customerName : $"Cliente #{sale.CustomerId}")}",
                $"Telefone: {(!string.IsNullOrWhiteSpace(customerPhone) ? customerPhone : "-")}",
                $"Entrega prevista: {sale.DeliveryDate.ToString("dd/MM", culture)} ({sale.DeliveryDate.ToString("dddd", culture)})",
                $"Status atual: {receiptStatusLine}",
                deliveredRecipients.Count > 0 ? $"Entregue à: {string.Join(", ", deliveredRecipients)}" : "Entregue à: -",
                string.Empty,
                "Resumo dos itens de reparo:",
                "------------------------------------------------------------"
            };

            decimal total = 0m;
            var entryItems = sale.EntryItems?.ToList() ?? new List<SaleEntryItem>();
            if (entryItems.Count == 0)
            {
                lines.Add("- Sem item para reparo.");
            }
            else
            {
                foreach (var entryItem in entryItems)
                {
                    var categoryName = !string.IsNullOrWhiteSpace(entryItem.Category?.Name)
                        ? entryItem.Category!.Name!
                        : $"Item #{entryItem.Id}";
                    lines.Add($"- {categoryName}");

                    if (!string.IsNullOrWhiteSpace(entryItem.ConditionNotes))
                        lines.Add($"  Estado da entrada: {entryItem.ConditionNotes.Trim()}");

                    var services = entryItem.Services?.ToList() ?? new List<SaleEntryItemService>();
                    if (services.Count == 0)
                    {
                        lines.Add("  Servicos: sem servico vinculado");
                        lines.Add(string.Empty);
                        continue;
                    }

                    foreach (var service in services)
                    {
                        var serviceName = service.ServiceItem?.Name?.Trim();
                        if (string.IsNullOrWhiteSpace(serviceName))
                            serviceName = $"Servico #{service.Id}";

                        var lineTotal = service.UnitPrice * Math.Max(service.Quantity, 1);

                        total += lineTotal;
                        lines.Add($"  Servico: {serviceName}");

                        if (!string.IsNullOrWhiteSpace(service.RepairDescription))
                            lines.Add($"  Reparo: {service.RepairDescription.Trim()}");

                        lines.Add($"  Valor: {lineTotal.ToString("C2", culture)}");
                    }

                    lines.Add(string.Empty);
                }
            }

            lines.Add("------------------------------------------------------------");
            lines.Add($"TOTAL REPAROS: {total.ToString("C2", culture)}");
            if (string.Equals(receiptStatusLine, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase))
            {
                lines.Add(string.Empty);
                lines.Add("Politica da loja:");
                lines.Add(BuildPolicyLine(
                    pickupPolicyText,
                    $"Retirada: buscar a peca em ate {Math.Max(pickupDeadlineDays ?? 30, 1)} dia(s)."));
                lines.Add(BuildPolicyLine(
                    adjustmentPolicyText,
                    $"Reparo: qualquer reclamacao ou reajuste deve ser solicitado em ate {Math.Max(adjustmentDeadlineDays ?? 7, 1)} dia(s) apos a retirada (sem cobranca)."));
            }
            return lines;
        }

        private static string BuildPolicyLine(string? customText, string fallbackText)
        {
            var value = (customText ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(value) ? fallbackText : value;
        }

        private static string BuildReceiptStatusLine(Sale sale)
        {
            var statuses = sale.EntryItems?
                .SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>())
                .Select(s => (s.ItemStatus ?? "Received").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList() ?? new List<string>();

            if (statuses.Count == 0)
                return "EM ANALISE";

            var hasReceived = statuses.Any(s => string.Equals(s, "Received", StringComparison.OrdinalIgnoreCase));
            var hasInRepair = statuses.Any(s => string.Equals(s, "InRepair", StringComparison.OrdinalIgnoreCase));
            var hasReady = statuses.Any(s => string.Equals(s, "Ready", StringComparison.OrdinalIgnoreCase));
            var hasDelivered = statuses.Any(s => string.Equals(s, "Delivered", StringComparison.OrdinalIgnoreCase));

            if (hasReady && !hasReceived && !hasInRepair)
                return "PRONTA PARA RETIRADA";

            if (hasDelivered && !hasReady && !hasReceived && !hasInRepair)
                return "ENTREGUE";

            if (hasInRepair)
                return "EM CONSERTO";

            if (hasReceived)
                return "RECEBIDA";

            return "EM ANDAMENTO";
        }

        private static string BuildStoreContactsLine(string? address, string? phone, string? email)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(address)) parts.Add(address.Trim());
            if (!string.IsNullOrWhiteSpace(phone)) parts.Add(phone.Trim());
            if (!string.IsNullOrWhiteSpace(email)) parts.Add(email.Trim());
            return parts.Count > 0 ? string.Join(" - ", parts) : "Endereco - Telefone - Email";
        }

        private static string SanitizeFileName(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
            var buffer = new List<char>(normalized.Length);
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                {
                    buffer.Add(c);
                }
                else if (char.IsWhiteSpace(c) || c == '.' || c == '/')
                {
                    buffer.Add('-');
                }
            }

            var compact = new string(buffer.ToArray());
            while (compact.Contains("--"))
                compact = compact.Replace("--", "-");
            compact = compact.Trim('-');
            if (compact.Length == 0) compact = "arquivo";
            return compact.Length > 60 ? compact[..60] : compact;
        }

        private string? ResolveAssetPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(_environment.ContentRootPath, path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
