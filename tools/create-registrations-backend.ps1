# Fix-Step34b-CustomDomainUI.ps1
# Anade el input de Custom Domain a los modales Create/Edit de Festivals.razor,
# y lo precarga en OpenEditModal. Ejecutar despues de Fix-Step34.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor"

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

# --- 1. Create modal: anadir input de Custom Domain tras Favicon URL ---
$results += Patch-File -Path $path -Description "Create modal: anadir Custom Domain" -OldString @'
                    <input class="form-input" placeholder="Favicon URL" @bind="createRequest.FaviconUrl" />
'@ -NewString @'
                    <input class="form-input" placeholder="Favicon URL" @bind="createRequest.FaviconUrl" />
                    <input class="form-input" placeholder="Custom Domain (ej. app.midominio.com)" @bind="createRequest.CustomDomain" />
'@

# --- 2. Edit modal: anadir input de Custom Domain tras Favicon URL ---
$results += Patch-File -Path $path -Description "Edit modal: anadir Custom Domain" -OldString @'
                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Favicon URL</label>
                        <input class="form-input" placeholder="Favicon URL" @bind="updateRequest.FaviconUrl" />
                    </div>
'@ -NewString @'
                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Favicon URL</label>
                        <input class="form-input" placeholder="Favicon URL" @bind="updateRequest.FaviconUrl" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Custom Domain</label>
                        <input class="form-input" placeholder="app.midominio.com" @bind="updateRequest.CustomDomain" />
                    </div>
'@

# --- 3. OpenEditModal: precargar CustomDomain ---
$results += Patch-File -Path $path -Description "OpenEditModal: precargar CustomDomain" -OldString @'
            FaviconUrl = festival.FaviconUrl,
'@ -NewString @'
            FaviconUrl = festival.FaviconUrl,
            CustomDomain = festival.CustomDomain,
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCampo Custom Domain anadido. dotnet build para confirmar." -ForegroundColor Green