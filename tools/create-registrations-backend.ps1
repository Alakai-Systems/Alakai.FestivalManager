# Fix-Step88-PersistentStateKeyCollision.ps1
#
# BUG REAL CONFIRMADO: App.razor y las 2 paginas de Login usan la MISMA clave
# ("branding") en PersistentComponentState, que se comparte entre TODOS los
# componentes del mismo circuito. Al intentar guardar/leer bajo el mismo
# nombre desde 2 sitios distintos en la misma peticion, chocan entre si.
#
# Fix: darle a cada uno su propia clave unica, sin tocar ninguna otra logica.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

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

$results = @()

# App.razor se queda con "branding" (no se toca).

# Auth/Login.razor -> "branding-participant-login"
$authPath = "Alakai.FestivalManager.Admin/Components/Pages/Auth/Login.razor"
$results += Patch-File -Path $authPath -Description "Auth/Login.razor: clave unica al leer" -OldString @'
        if (ApplicationState.TryTakeFromJson("branding", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
'@ -NewString @'
        if (ApplicationState.TryTakeFromJson("branding-participant-login", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
'@

$results += Patch-File -Path $authPath -Description "Auth/Login.razor: clave unica al guardar" -OldString @'
        ApplicationState.PersistAsJson("branding", Branding);
'@ -NewString @'
        ApplicationState.PersistAsJson("branding-participant-login", Branding);
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Auth/Login.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# AdminAuth/Login.razor -> "branding-admin-login"
$adminPath = "Alakai.FestivalManager.Admin/Components/Pages/AdminAuth/Login.razor"
$results2 = @()

$results2 += Patch-File -Path $adminPath -Description "AdminAuth/Login.razor: clave unica al leer" -OldString @'
        if (ApplicationState.TryTakeFromJson("branding", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
'@ -NewString @'
        if (ApplicationState.TryTakeFromJson("branding-admin-login", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
'@

$results2 += Patch-File -Path $adminPath -Description "AdminAuth/Login.razor: clave unica al guardar" -OldString @'
        ApplicationState.PersistAsJson("branding", Branding);
'@ -NewString @'
        ApplicationState.PersistAsJson("branding-admin-login", Branding);
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (AdminAuth/Login.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido - cada componente tiene ya su propia clave, sin colisiones. dotnet build para confirmar." -ForegroundColor Green