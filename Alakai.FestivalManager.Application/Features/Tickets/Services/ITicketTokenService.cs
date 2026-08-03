namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public interface ITicketTokenService
{
    /// <summary>Genera el token firmado (RegistrationId + firma HMAC) que se codifica en el QR del ticket.</summary>
    string GenerateToken(Guid registrationId);

    /// <summary>
    /// Valida un token leído de un QR y, si la firma es correcta, devuelve el RegistrationId.
    /// Pensado para la pantalla de check-in (fase posterior); no se usa todavía en ningún sitio.
    /// </summary>
    bool TryValidateToken(string token, out Guid registrationId);
}