<#
.SINOPSIS
  Fix (v2) - PaymentServiceTests.cs dejó de compilar al añadir ITicketService
  al constructor de PaymentService (paso 5).

  v2: la primera versión de este script falló en el paso 2 con
  "SKIP: anchor no encontrado" - no porque el contenido fuera distinto, sino
  porque ese archivo usa finales de línea CRLF y el anchor del script (escrito
  con LF) no coincidía byte a byte. La función Patch-File de este script ya
  normaliza CRLF/LF antes de comparar y respeta el final de línea original del
  archivo al guardar, así que esto no debería volver a pasar en ningún script
  de esta serie.

  Es idempotente respecto al script anterior: el paso de Global.cs ya se
  aplicó (lo verá como "ya aplicado" y lo saltará); solo falta el paso 2.

  Archivos que toca:
    - Alakai.FestivalManager.Tests\Global.cs
      (global using de Tickets.Services - ya aplicado, se saltará)
    - Alakai.FestivalManager.Tests\Unit\Application\Features\Payments\PaymentServiceTests.cs
      (añade Mock<ITicketService> y lo pasa al constructor)

.USO
  Ejecutar desde la raíz del repo:
    .\07-fix-payment-service-tests.ps1
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

    # Todo lo que sigue trabaja sobre versiones normalizadas a LF, para que dé
    # igual con qué final de línea se escribió este script.
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

$TestsProject = ".\Alakai.FestivalManager.Tests"

# ----------------------------------------------------------------------------
# 1. Global.cs (Tests) - global using de Tickets.Services (ya aplicado -> SKIP)
# ----------------------------------------------------------------------------
Patch-File `
    -Path "$TestsProject\Global.cs" `
    -Description "global using de Features.Tickets.Services en Tests" `
    -OldString @'
global using Alakai.FestivalManager.Application.Features.Payments.Services;
'@ `
    -NewString @'
global using Alakai.FestivalManager.Application.Features.Payments.Services;
global using Alakai.FestivalManager.Application.Features.Tickets.Services;
'@

# ----------------------------------------------------------------------------
# 2. PaymentServiceTests.cs - añadir el mock y pasarlo al constructor
# ----------------------------------------------------------------------------
Patch-File `
    -Path "$TestsProject\Unit\Application\Features\Payments\PaymentServiceTests.cs" `
    -Description "añade Mock<ITicketService> y lo pasa al constructor de PaymentService" `
    -OldString @'
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly Mock<IRedsysGateway> _redsysGateway = new();
    private readonly Mock<IEmailNotificationService> _emailService = new();
    private readonly Mock<ILogger<PaymentService>> _logger = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(
            _registrationRepo.Object,
            _redsysGateway.Object,
            _emailService.Object,
            _logger.Object);
    }
'@ `
    -NewString @'
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly Mock<IRedsysGateway> _redsysGateway = new();
    private readonly Mock<IEmailNotificationService> _emailService = new();
    private readonly Mock<ITicketService> _ticketService = new();
    private readonly Mock<ILogger<PaymentService>> _logger = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(
            _registrationRepo.Object,
            _redsysGateway.Object,
            _emailService.Object,
            _ticketService.Object,
            _logger.Object);
    }
'@

Write-Host ""
Write-Host "Fix completado." -ForegroundColor Cyan
Write-Host ""
Write-Host "VERIFICACIÓN:" -ForegroundColor Cyan
Write-Host "  1. dotnet build / dotnet test sobre Alakai.FestivalManager.Tests debe" -ForegroundColor Cyan
Write-Host "     compilar y pasar igual que antes." -ForegroundColor Cyan
Write-Host "  2. Si más adelante quieres verificar explícitamente que se llama a" -ForegroundColor Cyan
Write-Host "     EnsureTicketGeneratedAsync en el pago completo/segundo 50% y NO en" -ForegroundColor Cyan
Write-Host "     el primer 50%, dímelo y añadimos esas aserciones en un paso aparte." -ForegroundColor Cyan