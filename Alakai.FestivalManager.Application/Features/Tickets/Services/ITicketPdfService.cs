namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public class TicketInfo
{
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PassTypeName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
}

public interface ITicketPdfService
{
    /// <summary>Genera el PNG del QR (código estándar, legible por cualquier lector) a partir del token firmado.</summary>
    byte[] GenerateQrCode(string token);

    /// <summary>Genera el PDF del ticket con los datos básicos del registro y el QR ya insertado como imagen.</summary>
    byte[] GenerateTicketPdf(TicketInfo ticket, byte[] qrPngBytes);
}