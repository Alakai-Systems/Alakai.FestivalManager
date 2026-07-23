# Fix-Step82-ImpersonateRouteFix.ps1
# BUG REAL: la ruta de verdad del panel de usuario es
# "/user-panel/dashboard/{Lang}" (con el idioma obligatorio en la URL). La
# pagina de aterrizaje de impersonacion navegaba a "/user-panel/dashboard"
# sin idioma, lo que causaba un desajuste de ruta - de ahi el aspecto raro
# durante un instante y la posterior caida al login.
#
# Ejecutar DESPUES de Fix-Step78.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Impersonate.razor"

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

    if ($normalizedContent.Contains($normalizedNew)) {
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

$result = Patch-File -Path $path -Description "Navegar a la ruta real, con idioma incluido" -OldString @'
        Navigation.NavigateTo("/user-panel/dashboard", forceLoad: true);
'@ -NewString @'
        Navigation.NavigateTo("/user-panel/dashboard/en", forceLoad: true);
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de Impersonate.razor." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Nota: se navega siempre en ingles ('en') por defecto, ya que la impersonacion no conoce el idioma preferido del participante." -ForegroundColor Cyan