using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public class QuestPdfTicketService : ITicketPdfService
{
    private static readonly string AccentColor = "#6D5DD3";
    private static readonly string LineColor = Colors.Grey.Lighten2;
    private static readonly string MutedTextColor = Colors.Grey.Darken1;

    private sealed record TicketLabels(string HeaderTitle, string ParticipantLabel, string PassLabel, string Instructions);

    private static readonly Dictionary<string, TicketLabels> Translations = new()
    {
        ["es"] = new TicketLabels("ENTRADA", "PARTICIPANTE", "PASE", "Presenta este código QR el día del evento para hacer el check-in."),
        ["en"] = new TicketLabels("TICKET", "PARTICIPANT", "PASS", "Show this QR code on the day of the event to check in."),
        ["fr"] = new TicketLabels("BILLET", "PARTICIPANT", "PASS", "Présentez ce code QR le jour de l'événement pour faire le check-in."),
        ["ca"] = new TicketLabels("ENTRADA", "PARTICIPANT", "PASSI", "Presenta aquest codi QR el dia de l'esdeveniment per fer el check-in.")
    };

    private static TicketLabels GetLabels(string? language)
    {
        if (!string.IsNullOrWhiteSpace(language) && Translations.TryGetValue(language.ToLowerInvariant(), out TicketLabels? labels))
        {
            return labels;
        }

        return Translations["en"];
    }

    public byte[] GenerateQrCode(string token)
    {
        QRCodeGenerator qrGenerator = new();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public byte[] GenerateTicketPdf(TicketInfo ticket, byte[] qrPngBytes)
    {
        TicketLabels labels = GetLabels(ticket.Language);

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Column(headerColumn =>
                {
                    headerColumn.Item().AlignCenter().Text(labels.HeaderTitle).FontSize(22).Bold().FontColor(AccentColor);
                    headerColumn.Item().PaddingTop(4).AlignCenter().Text(ticket.EventName).FontSize(13).FontColor(MutedTextColor);
                    headerColumn.Item().PaddingTop(10).LineHorizontal(2).LineColor(AccentColor);
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Border(1).BorderColor(LineColor).Padding(12).Column(info =>
                    {
                        info.Item().Text(labels.ParticipantLabel).FontSize(9).Bold().FontColor(AccentColor);
                        info.Item().PaddingTop(4).Text(ticket.ParticipantName).FontSize(14).Bold();

                        info.Item().PaddingTop(10).Text(labels.PassLabel).FontSize(9).Bold().FontColor(AccentColor);
                        info.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(ticket.LevelName)
                            ? ticket.PassTypeName
                            : $"{ticket.PassTypeName} - {ticket.LevelName}");
                    });

                    column.Item().AlignCenter().Width(200).Image(qrPngBytes).FitWidth();

                    column.Item().AlignCenter().Text(labels.Instructions)
                        .FontSize(9).FontColor(MutedTextColor);
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(LineColor);
                    footer.Item().PaddingTop(6).AlignCenter().Text("Alakai Festival Manager").FontSize(8).FontColor(MutedTextColor);
                });
            });
        });

        return document.GeneratePdf();
    }
}