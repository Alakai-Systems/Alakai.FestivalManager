# Fix-Step16-CreateRegistrationHandlerTests.ps1
# El Paso 13 anadio IServiceScopeFactory al constructor de CreateRegistrationHandler.
# Este test todavia llamaba al constructor con la firma vieja.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Tests/Unit/Application/Features/Registrations/CreateRegistrationHandlerTests.cs"

function Patch-File {
    param(
        [string]$Path,
        [string]$OldString,
        [string]$NewString,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return $false
    }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"

    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)

    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$results = @()

$results += Patch-File -Path $path -Description "Anadir using Microsoft.Extensions.DependencyInjection" -OldString @'
using Alakai.FestivalManager.Application.Services.Security;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
using AutoMapper;
'@ -NewString @'
using Alakai.FestivalManager.Application.Services.Security;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
'@

$results += Patch-File -Path $path -Description "Anadir Mock<IServiceScopeFactory> y pasarlo al constructor" -OldString @'
    private readonly Mock<IRegistrationPartnerService> _partnerSvc = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateRegistrationHandler _sut;

    private readonly Edition _edition = new() { IsActive = true };
    private readonly PassType _passType = new();
    private readonly Level _level = new() { RegularPrice = 450m };

    public CreateRegistrationHandlerTests()
    {
        _sut = new CreateRegistrationHandler(
            _regRepo.Object, _editionRepo.Object, _passTypeRepo.Object, _levelRepo.Object,
            _userRepo.Object, _mapper.Object, _emailSvc.Object, _discountSvc.Object,
            _discountRepo.Object, _passwordHasher.Object, _partnerSvc.Object);
'@ -NewString @'
    private readonly Mock<IRegistrationPartnerService> _partnerSvc = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory = new();
    private readonly CreateRegistrationHandler _sut;

    private readonly Edition _edition = new() { IsActive = true };
    private readonly PassType _passType = new();
    private readonly Level _level = new() { RegularPrice = 450m };

    public CreateRegistrationHandlerTests()
    {
        _sut = new CreateRegistrationHandler(
            _regRepo.Object, _editionRepo.Object, _passTypeRepo.Object, _levelRepo.Object,
            _userRepo.Object, _mapper.Object, _emailSvc.Object, _discountSvc.Object,
            _discountRepo.Object, _passwordHasher.Object, _partnerSvc.Object, _serviceScopeFactory.Object);
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nTest corregido. dotnet build / dotnet test para confirmar." -ForegroundColor Green
Write-Host "Nota: como el envio de email ahora es fire-and-forget con su propio try/catch," -ForegroundColor Yellow
Write-Host "no hace falta configurar el mock de IServiceScopeFactory a fondo para este test -" -ForegroundColor Yellow
Write-Host "cualquier fallo ahi se traga silenciosamente, igual que en produccion." -ForegroundColor Yellow