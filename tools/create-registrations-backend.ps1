<#
.SINOPSIS
  Implementa lo que pediste: un QR que, leido con CUALQUIER lector de QR de un
  movil (camara del sistema, Google Lens, etc. - no solo la pantalla /checkin
  del Admin), muestre directamente los datos del registro y marque
  CheckedInAt en la base de datos, sin pasar por el login del Admin.

  COMO FUNCIONABA ANTES (el gap real, confirmado leyendo el codigo):
    - El QR del ticket codificaba el token en crudo (QuestPdfTicketService.
      GenerateQrCode(token)).
    - El unico endpoint que hacia el check-in (POST api/tickets/checkin) tiene
      [Authorize(Roles = "SuperAdmin,Admin")] - requiere estar logueado.
    - Un lector de QR generico solo puede mostrar texto o abrir una URL. Con
      un token en crudo, lo unico que puede hacer es mostrar el texto plano
      del token - no puede "iniciar sesion" ni llamar a un endpoint protegido.
      Por eso no hacia nada util fuera de la pantalla dedicada del Admin.

  QUE CAMBIA:
    1. El QR pasa a codificar una URL publica y sin login:
       https://<TicketSecurity:PublicApiBaseUrl>/checkin/{token}
    2. Esa URL la sirve un endpoint NUEVO, publico ([AllowAnonymous], sin
       [Authorize]), que llama al MISMO ITicketService.CheckInAsync que ya
       usa el Admin (sin tocar esa logica) y devuelve una pagina HTML sencilla
       con el resultado (verde = check-in ok, ambar = ya estaba, rojo =
       codigo invalido).
    3. La seguridad no depende del login: depende de que el token vaya firmado
       con HMAC (HmacTicketTokenService, sin cambios) - imposible de falsificar
       sin conocer TicketSecurity:SecretKey. Es el mismo modelo de confianza
       que ya tenias, solo que ahora no hace falta estar autenticado para
       validarlo, exactamente como pediste.
    4. La pantalla /checkin del Admin (camara dedicada) se deja tal cual y
       SIGUE funcionando iguial - se le anade solo una linea para que, si lo
       que escanea es la URL nueva, se quede con el token del final antes de
       llamar a la Api (asi no hace falta elegir entre una cosa u otra).

  IMPORTANTE - LIMITACION HONESTA: esto solo afecta a tickets generados A
  PARTIR de este cambio. Los tickets que ya existen y cuyo PDF sigue
  accesible (ya migrado a Blob Storage) mantienen su QR viejo (token en
  crudo) para siempre, porque EnsureTicketGeneratedAsync no regenera el PDF
  si el fichero ya existe - solo regenera si el fichero ha desaparecido. Si
  quieres que las entradas YA EMITIDAS tambien tengan el QR nuevo, dimelo y
  preparamos un modo de forzar la regeneracion.

  Archivos que modifica:
    - Application\Features\Tickets\Contracts\Settings\TicketSecurityOptions.cs  (+PublicApiBaseUrl)
    - Api\appsettings.json                                                      (+TicketSecurity:PublicApiBaseUrl)
    - Application\Features\Tickets\Services\TicketService.cs                    (construye la URL, no solo el token)
    - Api\Controllers\PublicCheckInController.cs                                (NUEVO - endpoint publico)
    - Admin\Components\Pages\CheckIn.razor                                      (acepta token en crudo O la URL nueva)

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\21-qr-public-checkin.ps1

  Despues:
    1. dotnet build (Api y Application, deben compilar limpio).
    2. Redeploy de app-alakai-swimout-api (y de app-alakai-swimout-admin, por
       el cambio en CheckIn.razor).
    3. En el Portal, App Service app-alakai-swimout-api, anade:
         Nombre: TicketSecurity__PublicApiBaseUrl
         Valor:  https://app-alakai-swimout-api.azurewebsites.net
       (si la Api tiene un dominio propio distinto de *.azurewebsites.net,
       usa ese en su lugar - dimelo si no estas segura de cual es).
    4. Verificacion: genera (o regenera, borrando el PDF viejo) un ticket de
       prueba, escanea su QR con la camara normal del movil (no la del Admin)
       y confirma que abre una pagina con el nombre del registro y que
       CheckedInAt se ha actualizado en BBDD.
#>

function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

# ============================================================================
# New-FileIfMissing v2: version corregida (la v1, usada desde el Script 10,
# tenia un bug real que doblaba los saltos de linea a "\r\r\n" - arreglado en
# el Script 18 con Repair-LineEndings. Esta version evita el bug desde el
# origen: normaliza a "\n" primero y expande a "\r\n" en un unico paso, asi
# que nunca puede haber un "\r" de mas.
# ============================================================================
function New-FileIfMissing {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$Content
    )

    if (Test-Path -LiteralPath $Path) {
        Write-Host "SKIP: el fichero ya existe -> $Path" -ForegroundColor DarkGray
        return
    }

    $directory = Split-Path -Path $Path -Parent
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $normalized = $Content -replace "`r`n", "`n"
    $finalContent = $normalized -replace "`n", "`r`n"

    [System.IO.File]::WriteAllText($Path, $finalContent, $utf8NoBom)
    Write-Host "OK: creado -> $Path" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$TicketSecurityOptionsPath = ".\Alakai.FestivalManager.Application\Features\Tickets\Contracts\Settings\TicketSecurityOptions.cs"
$ApiAppSettingsPath        = ".\Alakai.FestivalManager.Api\appsettings.json"
$TicketServicePath         = ".\Alakai.FestivalManager.Application\Features\Tickets\Services\TicketService.cs"
$PublicCheckInControllerPath = ".\Alakai.FestivalManager.Api\Controllers\PublicCheckInController.cs"
$CheckInRazorPath          = ".\Alakai.FestivalManager.Admin\Components\Pages\CheckIn.razor"

# ----------------------------------------------------------------------------
# 1. TicketSecurityOptions.cs: +PublicApiBaseUrl
# ----------------------------------------------------------------------------
Patch-File `
    -Path $TicketSecurityOptionsPath `
    -Description "+PublicApiBaseUrl" `
    -OldString @'
namespace Alakai.FestivalManager.Application.Features.Tickets.Contracts.Settings;

public class TicketSecurityOptions
{
    public string SecretKey { get; set; } = string.Empty;
}
'@ `
    -NewString @'
namespace Alakai.FestivalManager.Application.Features.Tickets.Contracts.Settings;

public class TicketSecurityOptions
{
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>URL publica base de la Api (ej. https://app-alakai-swimout-api.azurewebsites.net),
    /// usada para construir la URL de check-in que se codifica en el QR de la entrada.</summary>
    public string PublicApiBaseUrl { get; set; } = string.Empty;
}
'@

# ----------------------------------------------------------------------------
# 2. appsettings.json: +TicketSecurity:PublicApiBaseUrl
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiAppSettingsPath `
    -Description "+TicketSecurity:PublicApiBaseUrl" `
    -OldString @'
  "TicketSecurity": {
    "SecretKey": "CHANGE_THIS_TICKET_SECRET_KEY_IN_PRODUCTION_2026"
  },
'@ `
    -NewString @'
  "TicketSecurity": {
    "SecretKey": "CHANGE_THIS_TICKET_SECRET_KEY_IN_PRODUCTION_2026",
    "PublicApiBaseUrl": ""
  },
'@

# ----------------------------------------------------------------------------
# 3a. TicketService.cs: inyecta TicketSecurityOptions
# ----------------------------------------------------------------------------
Patch-File `
    -Path $TicketServicePath `
    -Description "inyecta TicketSecurityOptions" `
    -OldString @'
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ITicketTokenService _ticketTokenService;
    private readonly ITicketPdfService _ticketPdfService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IRegistrationRepository registrationRepository, ITicketTokenService ticketTokenService,
        ITicketPdfService ticketPdfService, IFileStorageService fileStorageService, ILogger<TicketService> logger)
    {
        _registrationRepository = registrationRepository;
        _ticketTokenService = ticketTokenService;
        _ticketPdfService = ticketPdfService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
'@ `
    -NewString @'
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ITicketTokenService _ticketTokenService;
    private readonly ITicketPdfService _ticketPdfService;
    private readonly IFileStorageService _fileStorageService;
    private readonly TicketSecurityOptions _ticketSecurityOptions;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IRegistrationRepository registrationRepository, ITicketTokenService ticketTokenService,
        ITicketPdfService ticketPdfService, IFileStorageService fileStorageService,
        IOptions<TicketSecurityOptions> ticketSecurityOptions, ILogger<TicketService> logger)
    {
        _registrationRepository = registrationRepository;
        _ticketTokenService = ticketTokenService;
        _ticketPdfService = ticketPdfService;
        _fileStorageService = fileStorageService;
        _ticketSecurityOptions = ticketSecurityOptions.Value;
        _logger = logger;
    }
'@

# ----------------------------------------------------------------------------
# 3b. TicketService.cs: el QR codifica la URL de check-in, no el token en crudo
# ----------------------------------------------------------------------------
Patch-File `
    -Path $TicketServicePath `
    -Description "QR codifica la URL de check-in" `
    -OldString @'
        string token = _ticketTokenService.GenerateToken(registration.Id);
        byte[] qrBytes = _ticketPdfService.GenerateQrCode(token);
'@ `
    -NewString @'
        string token = _ticketTokenService.GenerateToken(registration.Id);

        if (string.IsNullOrWhiteSpace(_ticketSecurityOptions.PublicApiBaseUrl))
        {
            _logger.LogWarning("TicketSecurity:PublicApiBaseUrl no esta configurado - el QR de check-in quedara incompleto.");
        }

        string checkInUrl = $"{_ticketSecurityOptions.PublicApiBaseUrl.TrimEnd('/')}/checkin/{token}";
        byte[] qrBytes = _ticketPdfService.GenerateQrCode(checkInUrl);
'@

# ----------------------------------------------------------------------------
# 4. Nuevo endpoint publico de check-in.
# ----------------------------------------------------------------------------
New-FileIfMissing `
    -Path $PublicCheckInControllerPath `
    -Content @'
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
'@

# ----------------------------------------------------------------------------
# 5a. CheckIn.razor: usa el token extraido (crudo o de la URL nueva)
# ----------------------------------------------------------------------------
Patch-File `
    -Path $CheckInRazorPath `
    -Description "usa ExtractTicketToken antes de llamar a la Api" `
    -OldString @'
    private async Task ProcessTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || isChecking)
        {
            return;
        }

        isChecking = true;
        lastResult = null;
        StateHasChanged();

        try
        {
            lastResult = await TicketsApiClient.CheckInAsync(token.Trim());
        }
'@ `
    -NewString @'
    private async Task ProcessTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || isChecking)
        {
            return;
        }

        string ticketToken = ExtractTicketToken(token.Trim());

        isChecking = true;
        lastResult = null;
        StateHasChanged();

        try
        {
            lastResult = await TicketsApiClient.CheckInAsync(ticketToken);
        }
'@

# ----------------------------------------------------------------------------
# 5b. CheckIn.razor: helper que extrae el token si lo escaneado es la URL nueva
# ----------------------------------------------------------------------------
Patch-File `
    -Path $CheckInRazorPath `
    -Description "anade ExtractTicketToken" `
    -OldString @'
    private async Task ResumeScannerAsync()
    {
'@ `
    -NewString @'
    private static string ExtractTicketToken(string scannedText)
    {
        // El QR de la entrada ahora codifica una URL publica de check-in
        // (https://.../checkin/{token}), no el token en crudo. Si lo que se
        // ha escaneado o pegado es esa URL, nos quedamos solo con el ultimo
        // segmento. Si es el token en crudo (entradas generadas antes de este
        // cambio), se usa tal cual - compatibilidad hacia atras.
        if (Uri.TryCreate(scannedText, UriKind.Absolute, out Uri? uri))
        {
            return uri.Segments.Length > 0 ? uri.Segments[^1].TrimEnd('/') : scannedText;
        }

        return scannedText;
    }

    private async Task ResumeScannerAsync()
    {
'@

Write-Host ""
Write-Host "Deberias ver 6 lineas 'OK' (5 aplicados + 1 creado)." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
Write-Host "  1. dotnet build (Api y Application, deben compilar limpio)." -ForegroundColor Cyan
Write-Host "  2. Redeploy de app-alakai-swimout-api Y app-alakai-swimout-admin." -ForegroundColor Cyan
Write-Host "  3. En el Portal, App Service app-alakai-swimout-api, anade:" -ForegroundColor Cyan
Write-Host "       Nombre: TicketSecurity__PublicApiBaseUrl" -ForegroundColor Cyan
Write-Host "       Valor:  https://app-alakai-swimout-api.azurewebsites.net" -ForegroundColor Cyan
Write-Host "     (si la Api tiene un dominio propio, usa ese en su lugar)." -ForegroundColor Cyan
Write-Host "  4. Los tickets YA EMITIDOS mantienen el QR viejo (token en crudo) - solo" -ForegroundColor Cyan
Write-Host "     los generados a partir de ahora tendran el QR nuevo. Dimelo si quieres" -ForegroundColor Cyan
Write-Host "     forzar la regeneracion de los ya emitidos." -ForegroundColor Cyan