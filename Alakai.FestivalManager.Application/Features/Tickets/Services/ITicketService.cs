namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public interface ITicketService
{
    /// <summary>
    /// Si el registro ya tiene TicketPdfUrl, no hace nada y devuelve la URL existente.
    /// Si no, y el registro está totalmente pagado, genera el token firmado, el QR y
    /// el PDF, guarda el archivo, actualiza Registration.TicketPdfUrl y devuelve la URL.
    /// Devuelve null si el registro no existe o todavía no está PaymentStatus.Paid.
    /// Pensado para llamarse desde cualquiera de los disparadores de confirmación de
    /// pago y desde el reenvío manual del email - es seguro llamarlo varias veces.
    /// </summary>
    Task<string?> EnsureTicketGeneratedAsync(Guid registrationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida el token firmado del QR. Si es invalido o no corresponde a ningun
    /// registro, devuelve null. Si es valido, marca Registration.CheckedInAt (si
    /// todavia no estaba marcado) y devuelve los datos del registro junto con
    /// AlreadyCheckedIn indicando si ya se habia hecho el check-in antes.
    /// </summary>
    Task<TicketCheckInResultDto?> CheckInAsync(string token, CancellationToken cancellationToken = default);
}