# Fix-DomainUserRoleProduction.ps1
#
# CAUSA REAL del "Validation failed" al crear un admin de Production:
#
# El enum que de verdad usa el backend para validar y generar el token es
# Alakai.FestivalManager.Domain.Enums.UserRole (SuperAdmin=1, Admin=2,
# User=3) - nunca tuvo un valor Production. Solo se anadio "Production=4"
# al enum AdminUserRole del proyecto Admin (usado unicamente para el menu
# y las paginas del Admin), que es un enum DISTINTO y separado.
#
# Por eso:
#   1. El validador CreateAdminUserCommandValidator solo permite
#      Admin/SuperAdmin -> "Validation failed" al mandar Production.
#   2. Aunque se saltara esa validacion, UserRole no tiene un miembro
#      llamado Production - "user.Role.ToString()" en el JWT devolveria
#      literalmente "4" en vez de "Production", y
#      [Authorize(Roles = "...,Production")] nunca reconoceria a ese
#      usuario.
#
# Este script arregla las dos cosas.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function Patch-File {
    param([string]$Path, [string]$OldString, [string]$NewString, [string]$Description)
    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return $false }
    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"
    if ($normalizedContent.Contains($normalizedNew)) { Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan; return $true }
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$results = @()

$results += Patch-File -Path "Alakai.FestivalManager.Domain/Enums/UserRole.cs" -Description "Domain UserRole: anadir Production = 4" -OldString @'
public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    User = 3
}
'@ -NewString @'
public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    User = 3,
    Production = 4
}
'@
if ($results -contains $false) { Write-Host "`nFallo (enum)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/Users/Validators/CreateAdminUserCommandValidator.cs" -Description "CreateAdminUserCommandValidator: permitir Production" -OldString @'
        RuleFor(command => command.Role)
            .Must(role => role == UserRole.Admin || role == UserRole.SuperAdmin)
            .WithMessage("Role must be Admin or SuperAdmin.");
'@ -NewString @'
        RuleFor(command => command.Role)
            .Must(role => role == UserRole.Admin || role == UserRole.SuperAdmin || role == UserRole.Production)
            .WithMessage("Role must be Admin, SuperAdmin or Production.");
'@
if ($results -contains $false) { Write-Host "`nFallo (validador)." -ForegroundColor Red; exit 1 }

Write-Host "`nAhora si se puede crear un admin de rol Production de verdad, y su token llevara el rol 'Production' correctamente." -ForegroundColor Green
Write-Host "No hace falta migracion - UserRole es un enum en memoria (se guarda como numero), no una tabla nueva." -ForegroundColor Yellow