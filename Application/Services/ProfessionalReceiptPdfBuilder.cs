using APISales.Domain.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace APISales.Application.Services
{
    public sealed class ReceiptPdfOptions
    {
        public string StoreName { get; set; } = "KuwenSys - Loja de Consertos";
        public string ReceiptTitle { get; set; } = "Ordem de Servico";
        public string StoreAddress { get; set; } = string.Empty;
        public string StorePhone { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public string? StoreLogoPath { get; set; }
        public string? AppLogoPath { get; set; }
        public int PickupDeadlineDays { get; set; } = 30;
        public int AdjustmentDeadlineDays { get; set; } = 5;
        public string PickupPolicyText { get; set; } = string.Empty;
        public string AdjustmentPolicyText { get; set; } = string.Empty;
    }

    public static class ProfessionalReceiptPdfBuilder
    {
        public static byte[] Build(Sale sale, ReceiptPdfOptions options)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var culture = new CultureInfo("pt-BR");
            var status = BuildReceiptStatusLine(sale);
            var customerName = string.IsNullOrWhiteSpace(sale.Customer?.Name) ? $"Cliente #{sale.CustomerId}" : sale.Customer!.Name!;
            var customerPhone = string.IsNullOrWhiteSpace(sale.Customer?.Phone) ? "-" : sale.Customer!.Phone!;

            var entryItems = (sale.EntryItems ?? new List<SaleEntryItem>()).ToList();
            var totalRepairs = entryItems
                .SelectMany(entry => entry.Services ?? new List<SaleEntryItemService>())
                .Sum(service => service.UnitPrice * Math.Max(service.Quantity, 1));
            var paymentStatusLabel = TranslatePaymentStatus(sale.PaymentStatus);
            var amountPaid = Math.Max(sale.AmountPaid, 0);
            var discountAmount = Math.Max(sale.DiscountAmount, 0);
            var finalTotal = Math.Max(totalRepairs - discountAmount, 0);
            var remainingAmount = Math.Max(finalTotal - amountPaid, 0);
            var contacts = string.Join(" - ", new[] { options.StoreAddress, options.StorePhone, options.StoreEmail }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

            byte[]? storeLogoBytes = null;
            if (!string.IsNullOrWhiteSpace(options.StoreLogoPath) && File.Exists(options.StoreLogoPath))
                storeLogoBytes = File.ReadAllBytes(options.StoreLogoPath);
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(9);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#1f2933"));
                    page.PageColor(Colors.White);

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Column(header =>
                        {
                            header.Spacing(3);
                            header.Item().Row(row =>
                            {
                                row.Spacing(8);
                                row.AutoItem().AlignMiddle().Element(container =>
                                {
                                    if (storeLogoBytes is not null)
                                        container.Height(28).Image(storeLogoBytes, ImageScaling.FitHeight);
                                    else
                                        container.Width(34).Height(22).Background("#4e20f1").AlignCenter().AlignMiddle().Text("KS").FontColor(Colors.White).SemiBold().FontSize(10);
                                });
                                row.RelativeItem().AlignMiddle().Column(title =>
                                {
                                    title.Item().Text(options.StoreName).SemiBold().FontColor("#312e81").FontSize(11);
                                    title.Item().Text($"{options.ReceiptTitle} #{sale.Id}").SemiBold().FontColor("#f14e20").FontSize(11);
                                });
                            });
                            if (!string.IsNullOrWhiteSpace(contacts))
                                header.Item().Text(contacts).FontSize(8.5f).FontColor("#52606d");
                        });

                        col.Item().Background("#f8f6ff").Padding(5).Column(meta =>
                        {
                            meta.Spacing(2);
                            meta.Item().Text($"{customerName} - Tel.: {customerPhone}").SemiBold().FontSize(9);
                            meta.Item().Text(BuildReceiptDateLine(sale, status, culture)).SemiBold().FontColor("#4e20f1");
                            if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase))
                                meta.Item().Text("Status: pronta para retirada").SemiBold().FontColor("#16a34a");
                            else if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
                            {
                                meta.Item().Text("Status: entregue").SemiBold().FontColor("#16a34a");
                                foreach (var line in BuildDeliveryDetailLines(sale))
                                    meta.Item().Text(line).FontSize(8.5f).FontColor("#52606d");
                            }
                        });

                        col.Item().Text("Itens").SemiBold().FontSize(11);

                        if (entryItems.Count == 0)
                        {
                            col.Item().Text("- Sem item de entrada.");
                        }
                        else
                        {
                            for (var i = 0; i < entryItems.Count; i++)
                            {
                                var entryItem = entryItems[i];
                                var isLastItem = i == entryItems.Count - 1;
                                col.Item().PaddingVertical(3).Element(itemContainer =>
                                {
                                    var content = isLastItem ? itemContainer : itemContainer.BorderBottom(1).BorderColor("#e5e7eb");
                                    content.Column(itemCol =>
                                {
                                    itemCol.Spacing(2);
                                    var category = string.IsNullOrWhiteSpace(entryItem.Category?.Name) ? $"Item #{entryItem.Id}" : entryItem.Category!.Name!;
                                    var audience = FormatAudienceType(entryItem.AudienceType);
                                    var entryDescription = string.IsNullOrWhiteSpace(entryItem.ConditionNotes) ? "-" : entryItem.ConditionNotes!.Trim();
                                    var services = (entryItem.Services ?? new List<SaleEntryItemService>()).ToList();
                                    var entryTotal = services.Sum(service => service.UnitPrice * Math.Max(service.Quantity, 1));

                                    itemCol.Item().Text($"{category} - {audience}").SemiBold().FontColor("#4e20f1").FontSize(11);
                                    itemCol.Item().Text($"Entrada: {entryDescription}").FontColor("#52606d");

                                    if (services.Count == 0)
                                    {
                                        itemCol.Item().Text("Servicos: sem servico vinculado");
                                    }
                                    else
                                    {
                                        foreach (var service in services)
                                        {
                                            var serviceName = string.IsNullOrWhiteSpace(service.ServiceItem?.Name) ? $"Servico #{service.Id}" : service.ServiceItem!.Name!;
                                            var serviceDescription = BuildServiceDescription(service);
                                            var lineTotal = service.UnitPrice * Math.Max(service.Quantity, 1);
                                            var serviceStatus = BuildServiceStatusText(service.ItemStatus);
                                            itemCol.Item().PaddingTop(2).Row(serviceRow =>
                                            {
                                                serviceRow.RelativeItem().Text(serviceName).SemiBold();
                                                serviceRow.AutoItem().Text(lineTotal.ToString("C2", culture)).SemiBold().FontColor("#1f2933");
                                            });
                                            itemCol.Item().Text(JoinParts(FormatActionType(service.ActionType), FormatServiceQuantity(service), serviceStatus));
                                            if (!string.IsNullOrWhiteSpace(serviceDescription))
                                                itemCol.Item().Text($"Obs.: {serviceDescription}").FontColor("#52606d");
                                        }
                                    }

                                    itemCol.Item().AlignRight().Text(entryTotal.ToString("C2", culture)).SemiBold().FontColor("#4e20f1").FontSize(11);
                                    });
                                });
                            }
                        }

                        col.Item().PaddingTop(2).AlignLeft().Column(totalBox =>
                        {
                            totalBox.Spacing(1);
                            totalBox.Item().Text("Total").SemiBold().FontColor("#4e20f1").FontSize(10);
                            totalBox.Item().Text(totalRepairs.ToString("C2", culture)).SemiBold().FontColor("#4e20f1").FontSize(15);
                            if (discountAmount > 0)
                                totalBox.Item().Text($"- Desconto: {discountAmount.ToString("C2", culture)}").SemiBold().FontColor("#fca5a5").FontSize(8.5f);
                            if (string.Equals(sale.PaymentStatus, "Partial", StringComparison.OrdinalIgnoreCase) && amountPaid > 0)
                            {
                                totalBox.Item().Text($"- Pago: {amountPaid.ToString("C2", culture)}").SemiBold().FontColor("#fca5a5").FontSize(8.5f);
                                totalBox.Item().Text($"Restante: {remainingAmount.ToString("C2", culture)}").SemiBold().FontColor("#f14e20").FontSize(9);
                            }
                            else if (discountAmount > 0)
                            {
                                totalBox.Item().Text($"Total final: {finalTotal.ToString("C2", culture)}").SemiBold().FontColor("#4e20f1").FontSize(9);
                            }
                            totalBox.Item().Text($"Pagamento: {paymentStatusLabel}").SemiBold().FontColor(GetPaymentStatusColor(sale.PaymentStatus)).FontSize(8.5f);
                        });

                        if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
                        {
                            col.Item().PaddingTop(2).Column(policy =>
                            {
                                policy.Spacing(2);
                                policy.Item().Text(BuildStatusPolicyLine(status, options)).FontSize(9);
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        private static string BuildReceiptDateLine(Sale sale, string status, CultureInfo culture)
        {
            if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
            {
                var deliveredAt = GetDeliveredAt(sale);
                if (deliveredAt.HasValue)
                    return $"Entregue: {deliveredAt.Value.ToString("dd/MM", culture)} ({deliveredAt.Value.ToString("dddd", culture)})";

                return "Entregue";
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
            var recipients = deliveredServices
                .Select(s => (s.DeliveredToName ?? string.Empty).Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var employees = deliveredServices
                .Select(s => (s.DeliveredByEmployee?.Name ?? string.Empty).Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var notes = deliveredServices
                .Select(s => (s.DeliveryNote ?? string.Empty).Trim())
                .Where(note => !string.IsNullOrWhiteSpace(note))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var lines = new List<string>();
            if (recipients.Count > 0)
                lines.Add($"Retirado por: {string.Join(", ", recipients)}");
            if (employees.Count > 0)
                lines.Add($"Entregue por: {string.Join(", ", employees)}");
            if (notes.Count > 0)
                lines.Add($"Obs. retirada: {string.Join(" | ", notes)}");

            return lines;
        }

        private static string BuildStatusPolicyLine(string status, ReceiptPdfOptions options)
        {
            if (string.Equals(status, "ENTREGUE", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(options.AdjustmentPolicyText))
                    return options.AdjustmentPolicyText.Trim();

                return $"Garantia do reparo: ajuste sem cobranca em ate {Math.Max(options.AdjustmentDeadlineDays, 1)} dia(s) apos a retirada.";
            }

            if (!string.IsNullOrWhiteSpace(options.PickupPolicyText))
                return options.PickupPolicyText.Trim();

            return $"Retirada: buscar a peca em ate {Math.Max(options.PickupDeadlineDays, 1)} dia(s) apos ficar pronta. Apos esse prazo, a peca podera ser descartada ou doada.";
        }

        private static DateTime? GetDeliveredAt(Sale sale)
        {
            if (sale.DeliveredAt.HasValue)
                return sale.DeliveredAt.Value;

            return GetDeliveredServices(sale)
                .Where(s => s.DeliveredAt.HasValue)
                .Select(s => (DateTime?)s.DeliveredAt!.Value)
                .Max();
        }

        private static DateTime? GetReadyAt(Sale sale)
        {
            if (sale.CompletedAt.HasValue)
                return sale.CompletedAt.Value;

            return GetAllServices(sale)
                .Where(s => s.ReadyAt.HasValue)
                .Select(s => (DateTime?)s.ReadyAt!.Value)
                .Max();
        }

        private static IEnumerable<SaleEntryItemService> GetDeliveredServices(Sale sale)
            => GetAllServices(sale)
                .Where(s => string.Equals(s.ItemStatus, "Delivered", StringComparison.OrdinalIgnoreCase));

        private static IEnumerable<SaleEntryItemService> GetAllServices(Sale sale)
            => sale.EntryItems?.SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>()) ?? Enumerable.Empty<SaleEntryItemService>();

        private static string BuildReceiptStatusLine(Sale sale)
        {
            if (string.Equals(sale.OrderStatus, "Delivered", StringComparison.OrdinalIgnoreCase))
                return "ENTREGUE";

            if (string.Equals(sale.OrderStatus, "Ready", StringComparison.OrdinalIgnoreCase))
                return "PRONTA PARA RETIRADA";

            var statuses = sale.EntryItems?
                .SelectMany(ei => ei.Services ?? new List<SaleEntryItemService>())
                .Select(s => (s.ItemStatus ?? "Received").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList() ?? new List<string>();

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
        {
            if (string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase))
                return "PAGO";
            if (string.Equals(status, "Partial", StringComparison.OrdinalIgnoreCase))
                return "PARCIAL";
            return "PENDENTE";
        }

        private static string GetPaymentStatusColor(string? status)
            => string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ? "#16a34a" : "#f14e20";

        private static string FormatAudienceType(string? audienceType)
            => string.Equals(audienceType, "Child", StringComparison.OrdinalIgnoreCase) ? "Infantil" : "Adulto";

        private static string FormatActionType(string? actionType)
            => (actionType ?? string.Empty).Trim() switch
            {
                "Replacement" => "Troca",
                "Addition" => "Inclusao",
                "Removal" => "Remocao",
                _ => "Ajuste",
            };

        private static string FormatServiceQuantity(SaleEntryItemService service)
        {
            var unit = (service.MeasurementUnit ?? "uni").Trim().ToLowerInvariant();
            if (unit == "cm" || unit == "m")
                return ExtractMeasureText(service.RepairDescription, unit) ?? $"{Math.Max(service.Quantity, 1)} {unit}";
            return $"{Math.Max(service.Quantity, 1)} uni";
        }

        private static string BuildServiceDescription(SaleEntryItemService service)
        {
            var unit = (service.MeasurementUnit ?? "uni").Trim().ToLowerInvariant();
            return DedupeDescriptionParts(service.RepairDescription, unit);
        }

        private static string DedupeDescriptionParts(string? value, string? measurementUnit = null)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return string.Join(" | ", (value ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !IsMeasureOnlyPart(part, measurementUnit))
                .Where(part => seen.Add(NormalizeText(part))));
        }

        private static string? ExtractMeasureText(string? value, string unit)
        {
            var parts = (value ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var normalized = NormalizeText(part);
                if (!ContainsMeasurementUnit(normalized, unit))
                    continue;
                var marker = part.LastIndexOf(':');
                var measure = marker >= 0 ? part[(marker + 1)..].Trim() : part.Trim();
                if (!string.IsNullOrWhiteSpace(measure))
                    return measure;
            }
            return null;
        }

        private static string NormalizeText(string value)
            => string.Join(" ", value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        private static bool IsMeasureOnlyPart(string value, string? unit)
        {
            var normalized = NormalizeText(value);
            var normalizedUnit = (unit ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedUnit != "cm" && normalizedUnit != "m")
                return false;

            return ContainsMeasurementUnit(normalized, normalizedUnit)
                && (normalized.Contains("barra") || normalized.Contains("gola") || normalized.Contains("medida"));
        }

        private static bool ContainsMeasurementUnit(string normalizedText, string unit)
            => normalizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(unit, StringComparer.OrdinalIgnoreCase);

        private static string TranslateServiceStatus(string? status)
            => (status ?? string.Empty).Trim() switch
            {
                "InRepair" => "Em conserto",
                "Ready" => "Pronta",
                "Delivered" => "Entregue",
                "Canceled" => "Cancelada",
                _ => "Recebida",
            };

        private static string? BuildServiceStatusText(string? status)
            => string.Equals(status, "Ready", StringComparison.OrdinalIgnoreCase) ? "Pronta" : null;

        private static string JoinParts(params string?[] parts)
            => string.Join(" - ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

    }
}
