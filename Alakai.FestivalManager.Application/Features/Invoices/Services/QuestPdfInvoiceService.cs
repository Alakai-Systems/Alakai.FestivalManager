using Alakai.FestivalManager.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class QuestPdfInvoiceService : IInvoicePdfService
{
    private static readonly string AccentColor = "#6D5DD3";
    private static readonly string LineColor = Colors.Grey.Lighten2;
    private static readonly string MutedTextColor = Colors.Grey.Darken1;

    public byte[] GenerateInvoicePdf(Invoice invoice, string eventName, string passTypeName, string participantName, InvoiceIssuerInfo issuer)
    {
        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Column(headerColumn =>
                {
                    headerColumn.Item().Row(row =>
                    {
                        if (issuer.LogoBytes is not null && issuer.LogoBytes.Length > 0)
                        {
                            row.ConstantItem(70).Height(70).Image(issuer.LogoBytes).FitArea();
                        }

                        row.RelativeItem().Column(column =>
                        {
                            column.Item().AlignRight().Text("INVOICE").FontSize(24).Bold().FontColor(AccentColor);
                            column.Item().AlignRight().PaddingTop(4).Text($"Number: {invoice.Number}").FontSize(10).FontColor(MutedTextColor);
                            column.Item().AlignRight().Text($"Date: {invoice.IssuedAt:dd/MM/yyyy}").FontSize(10).FontColor(MutedTextColor);
                        });
                    });

                    headerColumn.Item().PaddingTop(12).LineHorizontal(2).LineColor(AccentColor);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(16);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(LineColor).Padding(12).Column(from =>
                        {
                            from.Item().Text("FROM").FontSize(9).Bold().FontColor(AccentColor);
                            from.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(issuer.CompanyName) ? "-" : issuer.CompanyName).Bold();
                            if (!string.IsNullOrWhiteSpace(issuer.TaxId))
                            {
                                from.Item().Text($"Tax ID: {issuer.TaxId}").FontColor(MutedTextColor);
                            }
                            if (!string.IsNullOrWhiteSpace(issuer.Address))
                            {
                                from.Item().Text(issuer.Address).FontColor(MutedTextColor);
                            }
                            if (!string.IsNullOrWhiteSpace(issuer.City) || !string.IsNullOrWhiteSpace(issuer.Country))
                            {
                                from.Item().Text($"{issuer.PostalCode} {issuer.City}, {issuer.Country}").FontColor(MutedTextColor);
                            }
                        });

                        row.ConstantItem(16);

                        row.RelativeItem().Border(1).BorderColor(LineColor).Padding(12).Column(billed =>
                        {
                            billed.Item().Text("BILLED TO").FontSize(9).Bold().FontColor(AccentColor);
                            billed.Item().PaddingTop(4).Text(invoice.FiscalName).Bold();
                            billed.Item().Text($"Tax ID: {invoice.TaxId}").FontColor(MutedTextColor);
                            billed.Item().Text(invoice.Address).FontColor(MutedTextColor);
                            billed.Item().Text($"{invoice.PostalCode} {invoice.City}, {invoice.Country}").FontColor(MutedTextColor);
                        });
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(AccentColor).Padding(8).Text("Description").FontColor(Colors.White).Bold();
                            header.Cell().Background(AccentColor).Padding(8).Text("Participant").FontColor(Colors.White).Bold();
                            header.Cell().Background(AccentColor).Padding(8).AlignRight().Text("Amount").FontColor(Colors.White).Bold();
                        });

                        table.Cell().BorderBottom(1).BorderColor(LineColor).Padding(8).Text($"{eventName} - {passTypeName}");
                        table.Cell().BorderBottom(1).BorderColor(LineColor).Padding(8).Text(participantName);
                        table.Cell().BorderBottom(1).BorderColor(LineColor).Padding(8).AlignRight().Text($"{invoice.BaseAmount:0.00} EUR");
                    });

                    column.Item().AlignRight().Width(220).Column(totals =>
                    {
                        totals.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Taxable base").FontColor(MutedTextColor);
                            row.ConstantItem(90).AlignRight().Text($"{invoice.BaseAmount:0.00} EUR");
                        });
                        totals.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text($"VAT ({invoice.VatRate:0}%)").FontColor(MutedTextColor);
                            row.ConstantItem(90).AlignRight().Text($"{invoice.VatAmount:0.00} EUR");
                        });
                        totals.Item().PaddingTop(8).BorderTop(1).BorderColor(AccentColor).PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text("Total").FontSize(13).Bold();
                            row.ConstantItem(90).AlignRight().Text($"{invoice.Amount:0.00} EUR").FontSize(13).Bold().FontColor(AccentColor);
                        });
                    });
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(LineColor);
                    footer.Item().PaddingTop(6).AlignCenter().Text(text =>
                    {
                        string footerCompany = string.IsNullOrWhiteSpace(issuer.CompanyName) ? "Alakai Festival Manager" : issuer.CompanyName;
                        text.Span(footerCompany).FontSize(8).FontColor(MutedTextColor);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}