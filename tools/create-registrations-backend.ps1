# Fix-GeneralReportsMargin.ps1
#
# Mismo arreglo que ya se aplico a ProductionReports.razor, ahora en el
# Reports.razor general - quita el min-h forzado que dejaba un hueco en
# blanco despues de los filtros de festival/edicion.
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
$reportsPath = "Alakai.FestivalManager.Admin/Components/Pages/Reports.razor"

$results += Patch-File -Path $reportsPath -Description "Reports (general): quitar min-h[calc(100vh-212px)]" -OldString @'
<div class="grid grid-cols-1 gap-4 min-h-[calc(100vh-212px)]">
'@ -NewString @'
<div class="grid grid-cols-1 gap-4">
'@
if ($results -contains $false) { Write-Host "`nFallo." -ForegroundColor Red; exit 1 }

Write-Host "`nReports (general) sin el hueco en blanco tambien." -ForegroundColor Green