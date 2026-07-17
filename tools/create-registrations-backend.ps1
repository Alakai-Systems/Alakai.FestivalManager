# Fix-Step31-PartnerYesValueMismatch.ps1
# BUG REAL (preexistente): el <option> de "Si" tenia value="yes", pero TODAS
# las comprobaciones del codigo (linea 175, 439, 640) comparan contra "si".
# Por eso el campo de email de pareja nunca aparecia, sin importar que se
# eligiera "Si" en el desplegable.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Register.razor"

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

$result = Patch-File -Path $path -Description "Corregir value=yes -> value=si para que coincida con el resto del codigo" -OldString @'
                                <select class="form-select" @bind="WithPartner" @bind:after="OnWithPartnerChanged">
                                    <option value="">-</option>
                                    <option value="yes">@T.Get("yes")</option>
                                    <option value="no">No</option>
                                </select>
'@ -NewString @'
                                <select class="form-select" @bind="WithPartner" @bind:after="OnWithPartnerChanged">
                                    <option value="">-</option>
                                    <option value="si">@T.Get("yes")</option>
                                    <option value="no">No</option>
                                </select>
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual alrededor del select 'con pareja' en Register.razor." -ForegroundColor Red
    exit 1
}

Write-Host "`nBug corregido. dotnet build para confirmar." -ForegroundColor Green