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
        public int AdjustmentDeadlineDays { get; set; } = 7;
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

            var services = (sale.EntryItems ?? new List<SaleEntryItem>())
                .SelectMany(entry => (entry.Services ?? new List<SaleEntryItemService>())
                    .Select(service => new
                    {
                        Category = string.IsNullOrWhiteSpace(entry.Category?.Name) ? $"Item #{entry.Id}" : entry.Category!.Name!,
                        EntryDescription = string.IsNullOrWhiteSpace(entry.ConditionNotes) ? "-" : entry.ConditionNotes!,
                        ServiceName = string.IsNullOrWhiteSpace(service.ServiceItem?.Name) ? $"Servico #{service.Id}" : service.ServiceItem!.Name!,
                        Repair = string.IsNullOrWhiteSpace(service.RepairDescription) ? "-" : service.RepairDescription!,
                        Total = service.UnitPrice * Math.Max(service.Quantity, 1),
                    }))
                .ToList();

            var totalRepairs = services.Sum(x => x.Total);
            var paymentStatusLabel = TranslatePaymentStatus(sale.PaymentStatus);
            var amountPaid = Math.Max(sale.AmountPaid, 0);
            var contacts = string.Join(" - ", new[] { options.StoreAddress, options.StorePhone, options.StoreEmail }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));

            byte[]? storeLogoBytes = null;
            if (!string.IsNullOrWhiteSpace(options.StoreLogoPath) && File.Exists(options.StoreLogoPath))
                storeLogoBytes = File.ReadAllBytes(options.StoreLogoPath);
            byte[]? appLogoBytes = null;
            if (!string.IsNullOrWhiteSpace(options.AppLogoPath) && File.Exists(options.AppLogoPath))
                appLogoBytes = File.ReadAllBytes(options.AppLogoPath);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor("#1f2933"));
                    page.PageColor("#e7ebf0");

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().AlignCenter().Column(header =>
                        {
                            header.Spacing(4);
                            header.Item().AlignCenter().Row(row =>
                            {
                                row.Spacing(12);
                                row.AutoItem().AlignMiddle().Element(container =>
                                {
                                    if (storeLogoBytes is not null)
                                        container.Height(44).Image(storeLogoBytes, ImageScaling.FitHeight);
                                    else
                                        container.Width(44).Height(24).Background("#4e20f1").AlignCenter().AlignMiddle().Text("KS").FontColor(Colors.White).SemiBold();
                                });
                                row.AutoItem().AlignMiddle().Text(options.StoreName.ToUpperInvariant()).SemiBold().FontColor("#312e81").FontSize(17);
                            });
                            header.Item().AlignCenter().Text(options.ReceiptTitle).SemiBold().FontColor("#c2410c").FontSize(14);
                            if (!string.IsNullOrWhiteSpace(contacts))
                                header.Item().AlignCenter().Text(contacts).FontSize(11).FontColor("#f14e20");
                        });

                        col.Item().LineHorizontal(1).LineColor("#d9e2ec");

                        col.Item().PaddingTop(4).Column(meta =>
                        {
                            meta.Spacing(2);
                            meta.Item().Text($"Ordem #{sale.Id}").SemiBold();
                            meta.Item().Text($"Cliente: {customerName}");
                            meta.Item().Text($"Telefone: {customerPhone}");
                            meta.Item().Text($"Entrega prevista: {sale.DeliveryDate.ToString("dd/MM", culture)} ({sale.DeliveryDate.ToString("dddd", culture)})").FontColor("#4e20f1");
                            meta.Item().Text($"Status atual: {status}").SemiBold();
                        });

                        col.Item().PaddingTop(6).Text("Resumo dos itens de reparo:").SemiBold();
                        col.Item().LineHorizontal(1).LineColor("#d9e2ec");

                        if (services.Count == 0)
                        {
                            col.Item().PaddingVertical(6).Text("- Sem item para reparo.");
                        }
                        else
                        {
                            foreach (var item in services)
                            {
                                col.Item().PaddingTop(4).BorderBottom(1).BorderColor("#e5e7eb").PaddingBottom(6).Column(itemCol =>
                                {
                                    itemCol.Spacing(1);
                            itemCol.Item().Text(item.Category).SemiBold().FontColor("#4e20f1").FontSize(12.5f);
                                    itemCol.Item().Text($"Descricao de entrada: {item.EntryDescription}");
                                    itemCol.Item().Text($"Reparo: {item.ServiceName}");
                                    if (item.Repair != "-")
                                        itemCol.Item().Text($"Obs. reparo: {item.Repair}");
                                    itemCol.Item().AlignRight().Text(item.Total.ToString("C2", culture)).SemiBold();
                                });
                            }
                        }

                        col.Item().PaddingTop(4).Column(totals =>
                        {
                            totals.Spacing(2);
                            totals.Item().Text($"TOTAL REPAROS: {totalRepairs.ToString("C2", culture)}").SemiBold();
                            totals.Item().Text($"Status do pagamento: {paymentStatusLabel}").SemiBold();
                            if (string.Equals(sale.PaymentStatus, "Partial", StringComparison.OrdinalIgnoreCase))
                                totals.Item().Text($"Valor pago: {amountPaid.ToString("C2", culture)}").SemiBold();
                        });

                        if (string.Equals(status, "PRONTA PARA RETIRADA", StringComparison.OrdinalIgnoreCase))
                        {
                            var pickup = !string.IsNullOrWhiteSpace(options.PickupPolicyText)
                                ? options.PickupPolicyText
                                : $"Retirada: buscar a peca em ate {Math.Max(options.PickupDeadlineDays, 1)} dia(s).";
                            var adjustment = !string.IsNullOrWhiteSpace(options.AdjustmentPolicyText)
                                ? options.AdjustmentPolicyText
                                : $"Reparo: qualquer reclamacao ou reajuste deve ser solicitado em ate {Math.Max(options.AdjustmentDeadlineDays, 1)} dia(s) apos a retirada (sem cobranca).";

                            col.Item().PaddingTop(8).Column(policy =>
                            {
                                policy.Spacing(2);
                                policy.Item().Text("Politica da loja:").SemiBold();
                                policy.Item().Text(pickup).FontSize(11);
                                policy.Item().Text(adjustment).FontSize(11);
                            });
                        }
                    });

                    page.Footer().AlignCenter().Column(footer =>
                    {
                        footer.Spacing(2);
                        if (appLogoBytes is not null)
                            footer.Item().AlignCenter().Height(26).Image(appLogoBytes, ImageScaling.FitHeight);
                        else
                            footer.Item().Width(40).Height(18).Background("#4e20f1").AlignCenter().AlignMiddle().Text("KS").FontColor(Colors.White).SemiBold();
                        footer.Item().Text("KuwenSys").FontSize(11).FontColor("#1f2933").SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        private static string BuildReceiptStatusLine(Sale sale)
        {
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
    }
}
