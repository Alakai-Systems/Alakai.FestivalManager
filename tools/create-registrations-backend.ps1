# Fix-Step68-BiggerLogo.ps1
# Sube el logo de 40px a 70px de alto en ambos logins.
#
# Ejecutar DESPUES de Fix-Step67.
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

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Auth/Login.razor" `
    -Description "Auth: logo mas grande (40px -> 70px)" -OldString @'
            <div style="display:flex; align-items:center; justify-content:center; height:40px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:40px; object-fit:contain;" />
            </div>
'@ -NewString @'
            <div style="display:flex; align-items:center; justify-content:center; height:70px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:70px; object-fit:contain;" />
            </div>
'@

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/AdminAuth/Login.razor" `
    -Description "AdminAuth: logo mas grande (40px -> 70px)" -OldString @'
            <div style="display:flex; align-items:center; justify-content:center; height:40px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:40px; object-fit:contain;" />
            </div>
'@ -NewString @'
            <div style="display:flex; align-items:center; justify-content:center; height:70px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:70px; object-fit:contain;" />
            </div>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green