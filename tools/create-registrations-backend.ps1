<#
.SINOPSIS
  Fix - Los emails de PaymentConfirmed dejaron de llegar tras los pasos 5 y 6.

  Diagnóstico (con el código real leído de tu disco):

  1. EmailNotificationService.cs NUNCA llegó a modificarse - el Script 6 no
     se aplicó (probablemente el mismo problema de CRLF que vimos en
     PaymentServiceTests.cs, pero el Script 6 se generó antes de blindar
     Patch-File contra eso). Por eso el reenvío manual no adjunta PDF y no
     da ningún error: ese código sencillamente no existe todavía en el archivo.

  2. Fallo de diseño en PaymentService.cs: la llamada a
     ITicketService.EnsureTicketGeneratedAsync va ANTES del envío del email,
     dentro del mismo try/catch que envuelve todo el método. Si algo falla
     generando el ticket (QuestPDF, QRCoder, guardado del archivo - no se
     había probado nunca en real hasta ahora), la excepción aborta el método
     entero y el email NUNCA llega a intentarse - sin crear EmailLog, así
     que no hay ningún error visible en la tabla de emails, solo en el log
     de la aplicación.

  Este script arregla las dos cosas:
    A. Aplica de verdad los 3 cambios del Script 6 en EmailNotificationService.cs
       (con la función Patch-File ya blindada contra CRLF/LF), y esta vez el
       bloque que adjunta el ticket también queda envuelto en su propio
       try/catch - un fallo ahí ya no puede impedir que el email se envíe.
    B. Envuelve las dos llamadas a EnsureTicketGeneratedAsync en PaymentService.cs
       en un try/catch que solo registra un warning. Un fallo generando el
       ticket YA NUNCA bloqueará el email de confirmación de pago.

  Archivos que toca:
    - Alakai.FestivalManager.Application\Features\Emails\Services\EmailNotificationService.cs
    - Alakai.FestivalManager.Application\Features\Payments\Services\PaymentService.cs

  Idempotente: cada cambio se detecta y se salta si ya está aplicado.

.USO
  Ejecutar desde la raíz del repo:
    .\08-fix-blocked-emails.ps1
#>

# ============================================================================
# Patch-File v2: normaliza CRLF/LF antes de comparar y de contar ocurrencias,
# y respeta el final de línea original del archivo al escribir el resultado.
# ============================================================================
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

    $rawContent = Get-Content -LiteralPath $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized) -and -not $content.Contains($oldNormalized)) {
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

    Set-Content -LiteralPath $Path -Value $newContent -NoNewline
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$EmailServicePath   = ".\Alakai.FestivalManager.Application\Features\Emails\Services\EmailNotificationService.cs"
$PaymentServicePath = ".\Alakai.FestivalManager.Application\Features\Payments\Services\PaymentService.cs"

# ============================================================================
# PARTE A - aplicar de verdad el Script 6 en EmailNotificationService.cs
# ============================================================================

# ----------------------------------------------------------------------------
# A1. using de IFileStorageService
# ----------------------------------------------------------------------------
Patch-File `
    -Path $EmailServicePath `
    -Description "using de Features.Files.Services" `
    -OldString @'
using Alakai.FestivalManager.Infrastructure.Email;

namespace Alakai.FestivalManager.Application.Features.Emails.Services;
'@ `
    -NewString @'
using Alakai.FestivalManager.Application.Features.Files.Services;
using Alakai.FestivalManager.Infrastructure.Email;

namespace Alakai.FestivalManager.Application.Features.Emails.Services;
'@

# ----------------------------------------------------------------------------
# A2. Constructor - inyectar ITicketService e IFileStorageService
# ----------------------------------------------------------------------------
Patch-File `
    -Path $EmailServicePath `
    -Description "inyecta ITicketService e IFileStorageService en el constructor" `
    -OldString @'
    private readonly ApplicationUrlsOptions _applicationUrlsOptions;
    private readonly IMapper _mapper;
    private readonly SystemEmailOptions _systemEmailOptions;

    public EmailNotificationService(IEmailTemplateRepository emailTemplateRepository, IEmailLogRepository emailLogRepository, 
        IEmailTemplateRendererService emailTemplateRendererService, IMapper mapper, IRegistrationRepository registrationRepository,
        IEmailSender emailSender, IUserRepository userRepository, IEmailLayoutRepository emailLayoutRepository,
        IAccommodationReservationRepository accommodationReservationRepository, IBusReservationRepository busReservationRepository,
        IMealPreferenceRepository mealPreferenceRepository, IAccommodationBuildingRepository accommodationBuildingRepository,
        IOptions<SystemEmailOptions> systemEmailOptions, ICompetitionEntryRepository competitionEntryRepository,
        IOptions<ApplicationUrlsOptions> applicationUrlsOptions)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _emailLogRepository = emailLogRepository;
        _emailTemplateRendererService = emailTemplateRendererService;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
        _emailSender = emailSender;
        _userRepository = userRepository;
        _emailLayoutRepository = emailLayoutRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _busReservationRepository = busReservationRepository;
        _mealPreferenceRepository = mealPreferenceRepository;
        _accommodationBuildingRepository = accommodationBuildingRepository;
        _systemEmailOptions = systemEmailOptions.Value;
        _competitionEntryRepository = competitionEntryRepository;
        _applicationUrlsOptions = applicationUrlsOptions.Value;
    }
'@ `
    -NewString @'
    private readonly ApplicationUrlsOptions _applicationUrlsOptions;
    private readonly IMapper _mapper;
    private readonly SystemEmailOptions _systemEmailOptions;
    private readonly ITicketService _ticketService;
    private readonly IFileStorageService _fileStorageService;

    public EmailNotificationService(IEmailTemplateRepository emailTemplateRepository, IEmailLogRepository emailLogRepository, 
        IEmailTemplateRendererService emailTemplateRendererService, IMapper mapper, IRegistrationRepository registrationRepository,
        IEmailSender emailSender, IUserRepository userRepository, IEmailLayoutRepository emailLayoutRepository,
        IAccommodationReservationRepository accommodationReservationRepository, IBusReservationRepository busReservationRepository,
        IMealPreferenceRepository mealPreferenceRepository, IAccommodationBuildingRepository accommodationBuildingRepository,
        IOptions<SystemEmailOptions> systemEmailOptions, ICompetitionEntryRepository competitionEntryRepository,
        IOptions<ApplicationUrlsOptions> applicationUrlsOptions, ITicketService ticketService, IFileStorageService fileStorageService)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _emailLogRepository = emailLogRepository;
        _emailTemplateRendererService = emailTemplateRendererService;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
        _emailSender = emailSender;
        _userRepository = userRepository;
        _emailLayoutRepository = emailLayoutRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _busReservationRepository = busReservationRepository;
        _mealPreferenceRepository = mealPreferenceRepository;
        _accommodationBuildingRepository = accommodationBuildingRepository;
        _systemEmailOptions = systemEmailOptions.Value;
        _competitionEntryRepository = competitionEntryRepository;
        _applicationUrlsOptions = applicationUrlsOptions.Value;
        _ticketService = ticketService;
        _fileStorageService = fileStorageService;
    }
'@

# ----------------------------------------------------------------------------
# A3. CreateAndSendEmailAsync - asegurar y adjuntar el ticket cuando el
#     templateKey es PaymentConfirmed, con SU PROPIO try/catch para que un
#     fallo generando el ticket no impida enviar el email.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $EmailServicePath `
    -Description "asegura y adjunta el ticket PDF cuando templateKey es PaymentConfirmed (no bloqueante)" `
    -OldString @'
        try
        {
            EmailMessage message = new()
            {
                To = new EmailAddress
                {
                    Name = emailLog.RecipientName ?? string.Empty,
                    Address = emailLog.RecipientEmail
                },
                Subject = emailLog.Subject,
                HtmlBody = emailLog.BodyHtml,
                TextBody = emailLog.BodyText ?? string.Empty
            };

            await _emailSender.SendAsync(message, senderSettings, cancellationToken);
'@ `
    -NewString @'
        try
        {
            EmailMessage message = new()
            {
                To = new EmailAddress
                {
                    Name = emailLog.RecipientName ?? string.Empty,
                    Address = emailLog.RecipientEmail
                },
                Subject = emailLog.Subject,
                HtmlBody = emailLog.BodyHtml,
                TextBody = emailLog.BodyText ?? string.Empty
            };

            if (templateKey == EmailTemplateKey.PaymentConfirmed)
            {
                try
                {
                    string? ticketPdfUrl = await _ticketService.EnsureTicketGeneratedAsync(registrationId, cancellationToken);
                    string? ticketLocalPath = ticketPdfUrl is null ? null : _fileStorageService.ResolveLocalPath(ticketPdfUrl);

                    if (ticketLocalPath is not null && File.Exists(ticketLocalPath))
                    {
                        byte[] ticketBytes = await File.ReadAllBytesAsync(ticketLocalPath, cancellationToken);

                        message.Attachments.Add(new EmailAttachment
                        {
                            FileName = "ticket.pdf",
                            Content = ticketBytes,
                            ContentType = "application/pdf"
                        });
                    }
                }
                catch (Exception)
                {
                    // No dejamos que un fallo generando/adjuntando el ticket impida
                    // enviar el email de confirmación de pago en sí.
                }
            }

            await _emailSender.SendAsync(message, senderSettings, cancellationToken);
'@

# ============================================================================
# PARTE B - PaymentService.cs: que un fallo generando el ticket no bloquee
# nunca el envío del email de confirmación de pago.
# ============================================================================

# ----------------------------------------------------------------------------
# B1. ProcessRedsysReturnAsync
# ----------------------------------------------------------------------------
Patch-File `
    -Path $PaymentServicePath `
    -Description "hace no bloqueante EnsureTicketGeneratedAsync en ProcessRedsysReturnAsync" `
    -OldString @'
            if (becameFullyPaid)
            {
                await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
            }
'@ `
    -NewString @'
            if (becameFullyPaid)
            {
                try
                {
                    await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
                }
                catch (Exception ticketEx)
                {
                    _logger.LogWarning(ticketEx, "Could not generate the ticket for registration {RegistrationId}; the payment confirmation email will still be sent.", registration.Id);
                }
            }
'@

# ----------------------------------------------------------------------------
# B2. ProcessRedsysNotificationAsync
# ----------------------------------------------------------------------------
Patch-File `
    -Path $PaymentServicePath `
    -Description "hace no bloqueante EnsureTicketGeneratedAsync en ProcessRedsysNotificationAsync" `
    -OldString @'
        if (becameFullyPaid)
        {
            await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
        }
'@ `
    -NewString @'
        if (becameFullyPaid)
        {
            try
            {
                await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
            }
            catch (Exception ticketEx)
            {
                _logger.LogWarning(ticketEx, "Could not generate the ticket for registration {RegistrationId}; the payment confirmation email will still be sent.", registration.Id);
            }
        }
'@

Write-Host ""
Write-Host "Fix completado." -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE - revisa la salida de arriba línea por línea:" -ForegroundColor Yellow
Write-Host "  Debes ver 5 líneas 'OK: aplicado'. Si ves 'SKIP: anchor no encontrado'" -ForegroundColor Yellow
Write-Host "  en alguna, pégamela literal - significa que ese archivo no coincide" -ForegroundColor Yellow
Write-Host "  con lo que leí de tu repo y hay que mirarlo con más detalle." -ForegroundColor Yellow
Write-Host ""
Write-Host "VERIFICACIÓN:" -ForegroundColor Cyan
Write-Host "  1. dotnet build debe compilar limpio." -ForegroundColor Cyan
Write-Host "  2. Prueba un pago completo por Redsys: ahora el email debe llegar SIEMPRE," -ForegroundColor Cyan
Write-Host "     con o sin PDF adjunto. Si llega SIN PDF, revisa los logs de la Api" -ForegroundColor Cyan
Write-Host "     buscando 'Could not generate the ticket' - ese warning traerá el" -ForegroundColor Cyan
Write-Host "     error real de QuestPDF/QRCoder/guardado de archivo, y con eso ya" -ForegroundColor Cyan
Write-Host "     puedo arreglar la causa de fondo en un paso aparte." -ForegroundColor Cyan
Write-Host "  3. Prueba el reenvío manual - ahora sí debería adjuntar el PDF si" -ForegroundColor Cyan
Write-Host "     Registration.TicketPdfUrl ya está relleno en BD." -ForegroundColor Cyan