# Fix-CreateAdminUserHandlerProduction.ps1
#
# Habia una SEGUNDA comprobacion, independiente del validador, dentro del
# propio handler (defensa en profundidad - las dos existen a proposito,
# pero me deje una sin actualizar). Busque exhaustivamente en todo el repo
# el patron "!= UserRole.Admin && != UserRole.SuperAdmin" para confirmar
# que esta es la unica que falta.
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

$path = "Alakai.FestivalManager.Application/Features/Users/Commands/CreateAdminUser/CreateAdminUserHandler.cs"

$result = Patch-File -Path $path -Description "CreateAdminUserHandler: permitir Production" -OldString @'
        if (command.Role != UserRole.Admin && command.Role != UserRole.SuperAdmin)
        {
            throw new BusinessRuleException("Role must be Admin or SuperAdmin.");
        }
'@ -NewString @'
        if (command.Role != UserRole.Admin && command.Role != UserRole.SuperAdmin && command.Role != UserRole.Production)
        {
            throw new BusinessRuleException("Role must be Admin, SuperAdmin or Production.");
        }
'@

if (-not $result) { Write-Host "`nFallo." -ForegroundColor Red; exit 1 }

Write-Host "`nAhora si - las dos comprobaciones (validador + handler) permiten Production." -ForegroundColor Green