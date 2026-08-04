namespace Alakai.FestivalManager.Api.Controllers;

/// <summary>
/// Endpoint publico (sin login) que hace el check-in directamente cuando se
/// abre la URL codificada en el QR de la entrada. Pensado para que CUALQUIER
/// lector de QR de un movil (camara del sistema, Google Lens, etc.) - no solo
/// la pantalla /checkin del Admin - marque CheckedInAt en la base de datos y
/// muestre los datos del registro, sin pasar por el login del Admin.
///
/// La seguridad no depende de estar autenticado: depende de que el token del
/// QR este firmado con HMAC (ver HmacTicketTokenService) y sea imposible de
/// falsificar sin conocer TicketSecurity:SecretKey.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("checkin")]
public class PublicCheckInController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public PublicCheckInController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{token}")]
    public async Task<ContentResult> CheckIn(string token, CancellationToken cancellationToken)
    {
        TicketCheckInResultDto? result = await _ticketService.CheckInAsync(token, cancellationToken);

        string html = result is null
            ? BuildPage("invalid", "Codigo no valido", "El QR no corresponde a ninguna entrada valida.")
            : result.AlreadyCheckedIn
                ? BuildPage("already", "Ya se habia hecho check-in", BuildDetails(result))
                : BuildPage("success", "Check-in confirmado", BuildDetails(result));

        return new ContentResult
        {
            Content = html,
            ContentType = "text/html; charset=utf-8",
            StatusCode = 200
        };
    }

    private static string BuildDetails(TicketCheckInResultDto result)
    {
        string passLine = string.IsNullOrWhiteSpace(result.LevelName)
            ? result.PassTypeName
            : $"{result.PassTypeName} - {result.LevelName}";

        return $"<div class=\"name\">{WebUtility.HtmlEncode(result.ParticipantName)}</div>"
            + $"<div class=\"detail\">{WebUtility.HtmlEncode(result.EventName)} - {WebUtility.HtmlEncode(passLine)}</div>"
            + $"<div class=\"timestamp\">{result.CheckedInAt:dd/MM/yyyy HH:mm}</div>";
    }

    private static string BuildPage(string state, string title, string bodyHtml)
    {
        string accent = state switch
        {
            "success" => "#16a34a",
            "already" => "#d97706",
            _ => "#dc2626"
        };

        string background = state switch
        {
            "success" => "#f0fdf4",
            "already" => "#fffbeb",
            _ => "#fef2f2"
        };

        return "<!DOCTYPE html>"
            + "<html lang=\"es\"><head><meta charset=\"utf-8\">"
            + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
            + $"<title>{WebUtility.HtmlEncode(title)}</title>"
            + "<style>"
            + "body{font-family:-apple-system,Segoe UI,Roboto,sans-serif;background:#f4f4f5;margin:0;padding:24px;"
            + "display:flex;align-items:center;justify-content:center;min-height:100vh;}"
            + ".card{max-width:420px;width:100%;background:#fff;border-radius:16px;"
            + "box-shadow:0 4px 20px rgba(0,0,0,.08);padding:32px 24px;text-align:center;}"
            + $".badge{{display:inline-block;padding:8px 16px;border-radius:999px;font-weight:600;"
            + $"font-size:15px;color:{accent};background:{background};margin-bottom:16px;}}"
            + ".name{font-size:18px;font-weight:600;color:#111;margin-bottom:4px;}"
            + ".detail{font-size:15px;color:#333;margin-bottom:8px;}"
            + ".timestamp{font-size:13px;color:#777;}"
            + "</style></head><body>"
            + $"<div class=\"card\"><div class=\"badge\">{WebUtility.HtmlEncode(title)}</div>{bodyHtml}</div>"
            + "</body></html>";
    }
}