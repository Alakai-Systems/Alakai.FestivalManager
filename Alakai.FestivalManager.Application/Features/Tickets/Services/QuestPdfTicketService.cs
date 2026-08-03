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

    public byte[] GenerateQrCode(string token)
    {
        QRCodeGenerator qrGenerator = new();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public byte[] GenerateTicketPdf(TicketInfo ticket, byte[] qrPngBytes)
    {
        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Column(headerColumn =>
                {
                    headerColumn.Item().AlignCenter().Text("ENTRADA").FontSize(22).Bold().FontColor(AccentColor);
                    headerColumn.Item().PaddingTop(4).AlignCenter().Text(ticket.EventName).FontSize(13).FontColor(MutedTextColor);
                    headerColumn.Item().PaddingTop(10).LineHorizontal(2).LineColor(AccentColor);
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Border(1).BorderColor(LineColor).Padding(12).Column(info =>
                    {
                        info.Item().Text("PARTICIPANTE").FontSize(9).Bold().FontColor(AccentColor);
                        info.Item().PaddingTop(4).Text(ticket.ParticipantName).FontSize(14).Bold();

                        info.Item().PaddingTop(10).Text("PASE").FontSize(9).Bold().FontColor(AccentColor);
                        info.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(ticket.LevelName)
                            ? ticket.PassTypeName
                            : $"{ticket.PassTypeName} - {ticket.LevelName}");
                    });

                    column.Item().AlignCenter().Width(200).Image(qrPngBytes).FitWidth();

                    column.Item().AlignCenter().Text("Presenta este código QR el día del evento para hacer el check-in.")
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