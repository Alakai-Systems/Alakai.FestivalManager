# fix_auth_test.ps1
# Ejecutar desde la raiz del repo: .\tools\fix_auth_test.ps1

$ErrorActionPreference = "Stop"

$file = "Alakai.FestivalManager.Tests\Unit\Application\Features\Auth\AuthServiceLoginTests.cs"

if (-not (Test-Path $file)) {
    Write-Error "ABORT: No se encuentra $file"
    exit 1
}

function Patch-File {
    param([string]$Path, [string]$OldText, [string]$NewText, [string]$Description)
    $raw = [System.IO.File]::ReadAllText($Path)
    $rawNorm = $raw.Replace("`r`n", "`n")
    $oldNorm = $OldText.Replace("`r`n", "`n")
    $newNorm = $NewText.Replace("`r`n", "`n")
    $count = ([regex]::Matches($rawNorm, [regex]::Escape($oldNorm))).Count
    if ($count -ne 1) {
        Write-Error "ABORT: '$Description' — esperaba 1 en '$Path', encontradas $count"
        exit 1
    }
    $useCRLF = $raw.Contains("`r`n")
    $patched = $rawNorm.Replace($oldNorm, $newNorm)
    if ($useCRLF) { $patched = $patched.Replace("`n", "`r`n") }
    [System.IO.File]::WriteAllText($Path, $patched, [System.Text.Encoding]::UTF8)
    Write-Host "OK: $Description"
}

# 1. Añadir mock de IRegistrationRepository
Patch-File $file `
    '    private readonly Mock<ILogger<AuthService>> _logger = new();' `
    '    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();' `
    "AuthServiceLoginTests: añadir mock IRegistrationRepository"

# 2. Pasar el mock al constructor en el orden correcto
Patch-File $file `
    '        _sut = new AuthService(
            _authRepo.Object, _passwordSvc.Object, _jwtSvc.Object, _mapper.Object,
            _loginValidator.Object, _refreshValidator.Object, _forgotValidator.Object,
            _resetValidator.Object, _changeValidator.Object, _userRepo.Object,
            _emailSvc.Object, _externalAuth.Object, _logger.Object);' `
    '        _sut = new AuthService(
            _authRepo.Object, _passwordSvc.Object, _jwtSvc.Object, _mapper.Object,
            _loginValidator.Object, _refreshValidator.Object, _forgotValidator.Object,
            _resetValidator.Object, _changeValidator.Object, _userRepo.Object,
            _emailSvc.Object, _externalAuth.Object, _registrationRepo.Object, _logger.Object);' `
    "AuthServiceLoginTests: añadir _registrationRepo.Object al constructor"

Write-Host "Listo."