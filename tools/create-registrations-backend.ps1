# Fix-Step72-ReportsFilter.ps1
# Reports.razor tenia el mismo bug que las otras 6 paginas (falta w-full
# antes de md:wXX en los filtros).
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

$p = "Alakai.FestivalManager.Admin/Components/Pages/Reports.razor"
$results = @()
$results += Patch-File -Path $p -Description "Reports: filtro festival" -OldString '<select class="form-select md:w-56" @bind="selectedFestivalId" @bind:after="OnFestivalChangedAsync">' -NewString '<select class="form-select w-full md:w-56" @bind="selectedFestivalId" @bind:after="OnFestivalChangedAsync">'
$results += Patch-File -Path $p -Description "Reports: filtro edicion" -OldString '<select class="form-select md:w-64" @bind="selectedEditionId" @bind:after="OnEditionChangedAsync">' -NewString '<select class="form-select w-full md:w-64" @bind="selectedEditionId" @bind:after="OnEditionChangedAsync">'

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green