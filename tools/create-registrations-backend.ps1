<#
.SINOPSIS
  Arregla el error de compilacion que ha dado el Script 22: al anadir
  IFileStorageService como parametro nuevo de DeleteRegistrationHandler, el
  test DeleteRegistrationHandlerTests.cs (que lo instancia directamente con
  "new") se quedo con un argumento de menos:

    CS7036: There is no argument given that corresponds to the required
    parameter 'fileStorageService' of 'DeleteRegistrationHandler...'

  Culpa mia - se me paso ese test al escribir el Script 22. Este script anade
  el mock que falta y lo pasa al constructor.

  Archivo que modifica:
    - Alakai.FestivalManager.Tests\Unit\Application\Features\Registrations\DeleteRegistrationHandlerTests.cs

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\24-fix-delete-registration-test.ps1

  Despues: dotnet build (y si quieres, dotnet test) - debe compilar limpio.
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

$ErrorActionPreference = "Stop"

$TestPath = ".\Alakai.FestivalManager.Tests\Unit\Application\Features\Registrations\DeleteRegistrationHandlerTests.cs"

# ----------------------------------------------------------------------------
# 1. +using Files.Services
# ----------------------------------------------------------------------------
Patch-File `
    -Path $TestPath `
    -Description "+using Files.Services" `
    -OldString @'
using Alakai.FestivalManager.Application.Features.Emails.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
'@ `
    -NewString @'
using Alakai.FestivalManager.Application.Features.Emails.Services;
using Alakai.FestivalManager.Application.Features.Files.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
'@

# ----------------------------------------------------------------------------
# 2. +mock de IFileStorageService, pasado al constructor
# ----------------------------------------------------------------------------
Patch-File `
    -Path $TestPath `
    -Description "+mock IFileStorageService" `
    -OldString @'
    private readonly Mock<IInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<IEmailNotificationService> _emailSvc = new();
    private readonly DeleteRegistrationHandler _sut;

    public DeleteRegistrationHandlerTests()
    {
        _sut = new DeleteRegistrationHandler(
            _regRepo.Object, _compRepo.Object, _emailLogRepo.Object, _discountRepo.Object,
            _accomRepo.Object, _busRepo.Object, _invoiceRepo.Object, _emailSvc.Object);

        _compRepo.Setup(r => r.GetByPartnerRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _compRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _accomRepo.Setup(r => r.GetByResponsibleRegistrationIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AccommodationReservation?)null);
        _busRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _invoiceRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);
        _emailLogRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }
'@ `
    -NewString @'
    private readonly Mock<IInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<IEmailNotificationService> _emailSvc = new();
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly DeleteRegistrationHandler _sut;

    public DeleteRegistrationHandlerTests()
    {
        _sut = new DeleteRegistrationHandler(
            _regRepo.Object, _compRepo.Object, _emailLogRepo.Object, _discountRepo.Object,
            _accomRepo.Object, _busRepo.Object, _invoiceRepo.Object, _emailSvc.Object, _fileStorageService.Object);

        _compRepo.Setup(r => r.GetByPartnerRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _compRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _accomRepo.Setup(r => r.GetByResponsibleRegistrationIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AccommodationReservation?)null);
        _busRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _invoiceRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);
        _emailLogRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _fileStorageService.Setup(f => f.DeleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }
'@

Write-Host ""
Write-Host "Deberias ver 2 lineas 'OK: aplicado'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO: dotnet build (y dotnet test si quieres) - debe compilar limpio." -ForegroundColor Cyan