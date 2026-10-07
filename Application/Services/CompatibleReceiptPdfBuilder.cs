using APISales.Domain.Sales;
using System.Globalization;
using System.Text;

namespace APISales.Application.Services
{
    public static class CompatibleReceiptPdfBuilder
    {
        private const decimal PageWidth = 226.77m; // 80mm
        private const decimal Margin = 20m;
        private const decimal ContentWidth = PageWidth - (Margin * 2);

        public static byte[] Build(Sale sale, ReceiptPdfOptions options)
        {
            var culture = new CultureInfo("pt-BR");
            var entryItems = (sale.EntryItems ?? new List<SaleEntryItem>()).ToList();
            var services = entryItems.SelectMany(entry => entry.Services ?? new List<SaleEntryItemService>()).ToList();
            var pageHeight = EstimatePageHeight(entryItems, services);
            var writer = new PdfContentWriter(pageHeight);

            var status = BuildReceiptStatusLine(sale);
            var customerName = string.IsNullOrWhiteSpace(sale.Customer?.Name) ? $"Cliente #{sale.CustomerId}" : sale.Customer!.Name!;
            var customerPhone = string.IsNullOrWhiteSpace(sale.Customer?.Phone) ? "-" : sale.Customer!.Phone!;
            var totalRepairs = services.Sum(service => service.UnitPrice * Math.Max(service.Quantity, 1));
            var amountPaid = Math.Max(sale.AmountPaid, 0);
            var discountAmount = Math.Max(sale.DiscountAmount, 0);
            var finalTotal = Math.Max(totalRepairs - discountAmount, 0);
            var remainingAmount = Math.Max(finalTotal - amountPaid, 0);
            var contacts = string.Join(" - ", new[] { options.StoreAddress, options.StorePhone, options.StoreEmail }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

            writer.DrawLogo(Margin, writer.Y - 28);
            writer.Text(options.StoreName, Margin + 42, writer.Y - 7, 10, "#312e81", bold: true, maxChars: 23);
            writer.Text($"{options.ReceiptTitle} #{sale.Id}", Margin + 42, writer.Y - 22, 10, "#f14e20", bold: true, maxChars: 23);
            writer.MoveDown(34);

            if (!string.IsNullOrWhiteSpace(contacts))
                writer.Paragraph(contacts, 7.5m, "#52606d", maxChars: 38, lineHeight: 9);

            writer.MoveDown(6);
            writer.FilledRect(Margin, writer.Y - 44, ContentWidth, 44, "#f8f6ff");
            writer.MoveDown(8);
            writer.Paragraph($"{customerName} - Tel.: {customerPhone}", 8.5m, "#1f2933", maxChars: 35, lineHeight: 10, bold: true, insetX: 6);
            writer.Paragraph(BuildReceiptDateLine(sale, status, culture), 8.5m, "#4e20f1", maxChars: 35, lineHeight: 10, bold: true, insetX: 6);
            if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase))
                writer.Paragraph("Status: pronta para retirada", 8.5m, "#16a34a", maxChars: 35, lineHeight: 10, bold: true, insetX: 6);
            else if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
            {
                writer.Paragraph("Status: entregue", 8.5m, "#16a34a", maxChars: 35, lineHeight: 10, bold: true, insetX: 6);
                foreach (var line in BuildDeliveryDetailLines(sale))
                    writer.Paragraph(line, 7.5m, "#52606d", maxChars: 35, lineHeight: 9, insetX: 6);
            }
            writer.MoveDown(8);

            writer.Text("Itens", Margin, writer.Y, 10, "#1f2933", bold: true);
            writer.MoveDown(14);

            if (entryItems.Count == 0)
            {
                writer.Paragraph("- Sem item de entrada.", 9, "#1f2933", maxChars: 36, lineHeight: 11);
            }
            else
            {
                for (var i = 0; i < entryItems.Count; i++)
                {
                    var entryItem = entryItems[i];
                    var category = string.IsNullOrWhiteSpace(entryItem.Category?.Name) ? $"Item #{entryItem.Id}" : entryItem.Category!.Name!;
                    var audience = FormatAudienceType(entryItem.AudienceType);
                    var entryDescription = string.IsNullOrWhiteSpace(entryItem.ConditionNotes) ? "-" : entryItem.ConditionNotes!.Trim();
                    var itemServices = (entryItem.Services ?? new List<SaleEntryItemService>()).ToList();
                    var entryTotal = itemServices.Sum(service => service.UnitPrice * Math.Max(service.Quantity, 1));

                    writer.Paragraph($"{category} - {audience}", 9.5m, "#4e20f1", maxChars: 34, lineHeight: 11, bold: true);
                    writer.Paragraph($"Entrada: {entryDescription}", 8.5m, "#52606d", maxChars: 37, lineHeight: 10);

                    if (itemServices.Count == 0)
                    {
                        writer.Paragraph("Servicos: sem servico vinculado", 8.5m, "#1f2933", maxChars: 37, lineHeight: 10);
                    }
                    else
                    {
                        foreach (var service in itemServices)
                        {
                            var serviceName = string.IsNullOrWhiteSpace(service.ServiceItem?.Name) ? $"Servico #{service.Id}" : service.ServiceItem!.Name!;
                            var serviceDescription = BuildServiceDescription(service);
                            var lineTotal = service.UnitPrice * Math.Max(service.Quantity, 1);
                            var serviceStatus = BuildServiceStatusText(service.ItemStatus);

                            writer.Paragraph(serviceName, 8.5m, "#1f2933", maxChars: 27, lineHeight: 10, bold: true);
                            writer.Text(lineTotal.ToString("C2", culture), PageWidth - Margin, writer.Y + 10, 8.5m, "#1f2933", bold: true, alignRight: true);
                            writer.Paragraph(JoinParts(FormatActionType(service.ActionType), FormatServiceQuantity(service), serviceStatus), 8m, "#1f2933", maxChars: 37, lineHeight: 9);
                            if (!string.IsNullOrWhiteSpace(serviceDescription))
                                writer.Paragraph($"Obs.: {serviceDescription}", 8m, "#52606d", maxChars: 37, lineHeight: 9);
                            writer.MoveDown(2);
                        }
                    }

                    writer.Text(entryTotal.ToString("C2", culture), PageWidth - Margin, writer.Y, 9.5m, "#4e20f1", bold: true, alignRight: true);
                    writer.MoveDown(12);
                    if (i < entryItems.Count - 1)
                        writer.Line("#e5e7eb");
                }
            }

            writer.MoveDown(3);
            writer.Text("Total", Margin, writer.Y, 9, "#4e20f1", bold: true);
            writer.MoveDown(15);
            writer.Text(totalRepairs.ToString("C2", culture), Margin, writer.Y, 15, "#4e20f1", bold: true);
            writer.MoveDown(17);
            if (discountAmount > 0)
            {
                writer.Text($"- Desconto: {discountAmount.ToString("C2", culture)}", Margin, writer.Y, 8, "#f14e20", bold: true);
                writer.MoveDown(10);
            }
            if (string.Equals(sale.PaymentStatus, "Partial", StringComparison.OrdinalIgnoreCase) && amountPaid > 0)
            {
                writer.Text($"- Pago: {amountPaid.ToString("C2", culture)}", Margin, writer.Y, 8, "#f14e20", bold: true);
                writer.MoveDown(10);
                writer.Text($"Restante: {remainingAmount.ToString("C2", culture)}", Margin, writer.Y, 8.5m, "#f14e20", bold: true);
                writer.MoveDown(11);
            }
            else if (discountAmount > 0)
            {
                writer.Text($"Total final: {finalTotal.ToString("C2", culture)}", Margin, writer.Y, 8.5m, "#4e20f1", bold: true);
                writer.MoveDown(11);
            }
            writer.Text($"Pagamento: {TranslatePaymentStatus(sale.PaymentStatus)}", Margin, writer.Y, 8, GetPaymentStatusColor(sale.PaymentStatus), bold: true);
            writer.MoveDown(13);

            if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
            {
                writer.Paragraph(BuildStatusPolicyLine(status, options), 8, "#1f2933", maxChars: 39, lineHeight: 9);
            }

            return writer.BuildPdf();
        }

        private static decimal EstimatePageHeight(List<SaleEntryItem> entryItems, List<SaleEntryItemService> services)
            => Math.Max(360, 230 + (entryItems.Count * 48) + (services.Count * 45));

        private static string BuildReceiptDateLine(Sale sale, string status, CultureInfo culture)
        {
            if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
            {
                var deliveredAt = GetDeliveredAt(sale);
                return deliveredAt.HasValue ? $"Entregue: {deliveredAt.Value.ToString("dd/MM", culture)} ({deliveredAt.Value.ToString("dddd", culture)})" : "Entregue";
            }

            if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase))
            {
                var readyAt = GetReadyAt(sale);
                if (readyAt.HasValue)
                    return $"Pronto desde: {readyAt.Value.ToString("dd/MM", culture)} ({readyAt.Value.ToString("dddd", culture)})";
            }

            return $"Entrega prevista: {sale.DeliveryDate.ToString("dd/MM", culture)} ({sale.DeliveryDate.ToString("dddd", culture)})";
        }

        private static List<string> BuildDeliveryDetailLines(Sale sale)
        {
            var deliveredServices = GetDeliveredServices(sale).ToList();
            var recipients = deliveredServices.Select(s => (s.DeliveredToName ?? string.Empty).Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var employees = deliveredServices.Select(s => (s.DeliveredByEmployee?.Name ?? string.Empty).Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var notes = deliveredServices.Select(s => (s.DeliveryNote ?? string.Empty).Trim()).Where(note => !string.IsNullOrWhiteSpace(note)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var lines = new List<string>();
            if (recipients.Count > 0) lines.Add($"Retirado por: {string.Join(", ", recipients)}");
            if (employees.Count > 0) lines.Add($"Entregue por: {string.Join(", ", employees)}");
            if (notes.Count > 0) lines.Add($"Obs. retirada: {string.Join(" | ", notes)}");
            return lines;
        }

        private static string BuildStatusPolicyLine(string status, ReceiptPdfOptions options)
        {
            if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
                return !string.IsNullOrWhiteSpace(options.AdjustmentPolicyText)
                    ? options.AdjustmentPolicyText.Trim()
                    : $"Garantia do reparo: ajuste sem cobranca em ate {Math.Max(options.AdjustmentDeadlineDays, 1)} dia(s) apos a retirada.";

            return !string.IsNullOrWhiteSpace(options.PickupPolicyText)
                ? options.PickupPolicyText.Trim()
                : $"Retirada: buscar a peca em ate {Math.Max(options.PickupDeadlineDays, 1)} dia(s) apos ficar pronta. Apos esse prazo, a peca podera ser descartada ou doada.";
        }

        private static DateTime? GetDeliveredAt(Sale sale)
            => sale.DeliveredAt ?? GetDeliveredServices(sale).Where(s => s.DeliveredAt.HasValue).Select(s => (DateTime?)s.DeliveredAt!.Value).Max();

        private static DateTime? GetReadyAt(Sale sale)
            => sale.CompletedAt ?? GetAllServices(sale).Where(s => s.ReadyAt.HasValue).Select(s => (DateTime?)s.ReadyAt!.Value).Max();

        private static IEnumerable<SaleEntryItemService> GetDeliveredServices(Sale sale)
            => GetAllServices(sale).Where(s => string.Equals(s.ItemStatus, "Delivered", StringComparison.OrdinalIgnoreCase));

        private static IEnumerable<SaleEntryItemService> GetAllServices(Sale sale)
            => sale.EntryItems?.SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>()) ?? Enumerable.Empty<SaleEntryItemService>();

        private static string BuildReceiptStatusLine(Sale sale)
        {
            if (string.Equals(sale.OrderStatus, "Delivered", StringComparison.OrdinalIgnoreCase)) return "ENTREGUE";
            if (string.Equals(sale.OrderStatus, "Ready", StringComparison.OrdinalIgnoreCase)) return "PRONTA PARA RETIRADA";

            var statuses = sale.EntryItems?.SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>()).Select(s => (s.ItemStatus ?? "Received").Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>();
            if (statuses.Count == 0) return "EM ANALISE";
            var hasReceived = statuses.Any(s => string.Equals(s, "Received", StringComparison.OrdinalIgnoreCase));
            var hasInRepair = statuses.Any(s => string.Equals(s, "InRepair", StringComparison.OrdinalIgnoreCase));
            var hasReady = statuses.Any(s => string.Equals(s, "Ready", StringComparison.OrdinalIgnoreCase));
            var hasDelivered = statuses.Any(s => string.Equals(s, "Delivered", StringComparison.OrdinalIgnoreCase));
            if (hasReady && !hasReceived && !hasInRepair) return "PRONTA PARA RETIRADA";
            if (hasDelivered && !hasReady && !hasReceived && !hasInRepair) return "ENTREGUE";
            if (hasInRepair) return "EM CONSERTO";
            if (hasReceived) return "RECEBIDA";
            return "EM ANDAMENTO";
        }

        private static string TranslatePaymentStatus(string? status)
            => string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ? "PAGO" : string.Equals(status, "Partial", StringComparison.OrdinalIgnoreCase) ? "PARCIAL" : "PENDENTE";

        private static string GetPaymentStatusColor(string? status)
            => string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ? "#16a34a" : "#f14e20";

        private static string FormatAudienceType(string? audienceType)
            => string.Equals(audienceType, "Child", StringComparison.OrdinalIgnoreCase) ? "Infantil" : "Adulto";

        private static string FormatActionType(string? actionType)
            => (actionType ?? string.Empty).Trim() switch { "Replacement" => "Troca", "Addition" => "Inclusao", "Removal" => "Remocao", _ => "Ajuste" };

        private static string FormatServiceQuantity(SaleEntryItemService service)
        {
            var unit = (service.MeasurementUnit ?? "uni").Trim().ToLowerInvariant();
            if (unit == "cm" || unit == "m")
                return ExtractMeasureText(service.RepairDescription, unit) ?? $"{Math.Max(service.Quantity, 1)} {unit}";
            return $"{Math.Max(service.Quantity, 1)} uni";
        }

        private static string BuildServiceDescription(SaleEntryItemService service)
            => DedupeDescriptionParts(service.RepairDescription, (service.MeasurementUnit ?? "uni").Trim().ToLowerInvariant());

        private static string DedupeDescriptionParts(string? value, string? measurementUnit = null)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return string.Join(" | ", (value ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(part => !IsMeasureOnlyPart(part, measurementUnit)).Where(part => seen.Add(NormalizeText(part))));
        }

        private static string? ExtractMeasureText(string? value, string unit)
        {
            foreach (var part in (value ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var normalized = NormalizeText(part);
                if (!ContainsMeasurementUnit(normalized, unit)) continue;
                var marker = part.LastIndexOf(':');
                var measure = marker >= 0 ? part[(marker + 1)..].Trim() : part.Trim();
                if (!string.IsNullOrWhiteSpace(measure)) return measure;
            }
            return null;
        }

        private static string NormalizeText(string value)
            => string.Join(" ", value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        private static bool IsMeasureOnlyPart(string value, string? unit)
        {
            var normalizedUnit = (unit ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedUnit != "cm" && normalizedUnit != "m") return false;
            var normalized = NormalizeText(value);
            return ContainsMeasurementUnit(normalized, normalizedUnit) && (normalized.Contains("barra") || normalized.Contains("gola") || normalized.Contains("medida"));
        }

        private static bool ContainsMeasurementUnit(string normalizedText, string unit)
            => normalizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(unit, StringComparer.OrdinalIgnoreCase);

        private static string? BuildServiceStatusText(string? status)
            => string.Equals(status, "Ready", StringComparison.OrdinalIgnoreCase) ? "Pronta" : null;

        private static string JoinParts(params string?[] parts)
            => string.Join(" - ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

        private sealed class PdfContentWriter
        {
            private readonly StringBuilder _content = new();
            private readonly decimal _pageHeight;

            public decimal Y { get; private set; }

            public PdfContentWriter(decimal pageHeight)
            {
                _pageHeight = pageHeight;
                Y = pageHeight - Margin;
            }

            public void MoveDown(decimal value) => Y -= value;

            public void DrawLogo(decimal x, decimal y)
            {
                FilledRect(x + 9, y, 12, 34, "#f6c344");
                FilledRect(x + 5, y + 33, 20, 3, "#f6c344");
                Text("KS", x + 2, y + 15, 9, "#f6c344", bold: true);
            }

            public void Line(string color)
            {
                SetColor(color);
                _content.AppendLine($"{F(Margin)} {F(Y)} m {F(PageWidth - Margin)} {F(Y)} l S");
                MoveDown(8);
            }

            public void FilledRect(decimal x, decimal y, decimal width, decimal height, string color)
            {
                SetFillColor(color);
                _content.AppendLine($"{F(x)} {F(y)} {F(width)} {F(height)} re f");
            }

            public void Paragraph(string text, decimal size, string color, int maxChars, decimal lineHeight, bool bold = false, decimal insetX = 0)
            {
                foreach (var line in Wrap(text, maxChars))
                {
                    Text(line, Margin + insetX, Y, size, color, bold);
                    MoveDown(lineHeight);
                }
            }

            public void Text(string text, decimal x, decimal y, decimal size, string color, bool bold = false, bool alignRight = false, int? maxChars = null)
            {
                var value = maxChars.HasValue && text.Length > maxChars.Value ? text[..Math.Max(maxChars.Value - 1, 1)] + "." : text;
                if (alignRight)
                    x -= EstimateTextWidth(value, size);

                _content.AppendLine("BT");
                _content.AppendLine($"/{(bold ? "F2" : "F1")} {F(size)} Tf");
                SetTextColor(color);
                _content.AppendLine($"{F(x)} {F(y)} Td");
                _content.AppendLine($"({Escape(value)}) Tj");
                _content.AppendLine("ET");
            }

            public byte[] BuildPdf()
            {
                var contentStream = EncodeLatin1(_content.ToString());
                var objects = new List<byte[]>
                {
                    EncodeLatin1("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
                    EncodeLatin1("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
                    EncodeLatin1($"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(PageWidth)} {F(_pageHeight)}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>\nendobj\n"),
                    EncodeLatin1("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n"),
                    EncodeLatin1("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n"),
                    BuildContentObject(contentStream),
                };

                using var stream = new MemoryStream();
                Write(stream, EncodeLatin1("%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n"));
                var offsets = new List<long> { 0 };
                foreach (var obj in objects)
                {
                    offsets.Add(stream.Position);
                    Write(stream, obj);
                }

                var xrefPosition = stream.Position;
                Write(stream, EncodeLatin1($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n"));
                for (var i = 1; i <= objects.Count; i++)
                    Write(stream, EncodeLatin1($"{offsets[i]:D10} 00000 n \n"));
                Write(stream, EncodeLatin1($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF"));
                return stream.ToArray();
            }

            private static byte[] BuildContentObject(byte[] contentStream)
            {
                using var stream = new MemoryStream();
                Write(stream, EncodeLatin1($"6 0 obj\n<< /Length {contentStream.Length} >>\nstream\n"));
                Write(stream, contentStream);
                Write(stream, EncodeLatin1("endstream\nendobj\n"));
                return stream.ToArray();
            }

            private void SetColor(string hex)
            {
                var (r, g, b) = Rgb(hex);
                _content.AppendLine($"{F(r)} {F(g)} {F(b)} RG");
            }

            private void SetFillColor(string hex)
            {
                var (r, g, b) = Rgb(hex);
                _content.AppendLine($"{F(r)} {F(g)} {F(b)} rg");
            }

            private void SetTextColor(string hex)
            {
                var (r, g, b) = Rgb(hex);
                _content.AppendLine($"{F(r)} {F(g)} {F(b)} rg");
            }

            private static IEnumerable<string> Wrap(string text, int maxChars)
            {
                var value = (text ?? string.Empty).Trim();
                if (value.Length <= maxChars) return new[] { value };
                var lines = new List<string>();
                while (value.Length > maxChars)
                {
                    var splitAt = value.LastIndexOf(' ', maxChars);
                    if (splitAt <= 0) splitAt = maxChars;
                    lines.Add(value[..splitAt].Trim());
                    value = value[splitAt..].Trim();
                }
                if (!string.IsNullOrWhiteSpace(value)) lines.Add(value);
                return lines;
            }

            private static decimal EstimateTextWidth(string text, decimal size) => text.Length * size * 0.48m;

            private static (decimal R, decimal G, decimal B) Rgb(string hex)
            {
                var value = hex.TrimStart('#');
                return (
                    Convert.ToInt32(value[..2], 16) / 255m,
                    Convert.ToInt32(value.Substring(2, 2), 16) / 255m,
                    Convert.ToInt32(value.Substring(4, 2), 16) / 255m);
            }

            private static string Escape(string value)
            {
                var safe = (value ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
                return Encoding.Latin1.GetString(Encoding.Latin1.GetBytes(safe));
            }

            private static string F(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
            private static byte[] EncodeLatin1(string text) => Encoding.Latin1.GetBytes(text);
            private static void Write(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);
        }
    }
}
