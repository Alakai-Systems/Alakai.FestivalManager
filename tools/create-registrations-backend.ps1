# Fix-AccommodationBuildingManageHeaderOverflow.ps1
#
# La fila de tipo/bloqueo en el lado de participantes tiene una
# estructura distinta a la de Produccion (un div interno extra con
# gap-3, y un badge condicional de "Allowed: ..."), asi que el ancla
# generica no encajaba. Ancla exacta para esta version.
#
# Ejecutar desde la raiz del repo, DESPUES de
# Fix-ProductionBuildingManagePageOverflow-v2.ps1 (ese ya arreglo bien
# los otros 3 puntos, este es solo el que fallo).
$ErrorActionPreference = "Stop"

$path = "Alakai.FestivalManager.Admin/Components/Pages/AccommodationBuildingManage.razor"

if (-not (Test-Path $path)) {
    Write-Host "SKIP (archivo no encontrado): $path" -ForegroundColor Yellow
    exit 1
}

$rawContent = Get-Content -Path $path -Raw
$usesCrlf = $rawContent.Contains("`r`n")
$normalizedContent = $rawContent -replace "`r`n", "`n"

$old = @'
    <div class="flex items-center justify-between mt-4 card">
        <div class="flex items-center gap-3">
'@ -replace "`r`n", "`n"

$new = @'
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between mt-4 card">
        <div class="flex flex-wrap items-center gap-3">
'@ -replace "`r`n", "`n"

if ($normalizedContent.Contains($new) -and -not $normalizedContent.Contains($old)) {
    Write-Host "SKIP (ya aplicado)" -ForegroundColor Cyan
}
elseif ($normalizedContent.Contains($old)) {
    $updatedNormalized = $normalizedContent.Replace($old, $new)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $path -Value $updatedFinal -NoNewline
    Write-Host "OK: fila de tipo/bloqueo (participantes) con wrap responsive" -ForegroundColor Green
}
else {
    Write-Host "SKIP (anchor no encontrado)" -ForegroundColor Yellow
    exit 1
}