# Fix-Step87-InternalDashboardCalls.ps1
# Corrige las 5 llamadas internas a GetDashboardAsync(userId, cancellationToken)
# dentro de UserPanelService.cs (usadas por otros metodos para devolver el
# panel actualizado tras un cambio) - todas identicas, todas necesitan el
# mismo arreglo: anadir "null" como dominio (sin cambio de comportamiento en
# esos metodos todavia, se corrigen de verdad en el siguiente paso).
#
# Ejecutar DESPUES de Fix-Step86.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs"

if (-not (Test-Path $path)) {
    Write-Host "SKIP (archivo no encontrado): $path" -ForegroundColor Yellow
    exit 1
}

$content = Get-Content -Path $path -Raw
$before = "GetDashboardAsync(userId, cancellationToken)"
$after = "GetDashboardAsync(userId, null, cancellationToken)"

$occurrences = ([regex]::Matches($content, [regex]::Escape($before))).Count

if ($occurrences -eq 0) {
    Write-Host "SKIP: no se encontro ninguna llamada con la firma antigua (puede que ya se haya aplicado)." -ForegroundColor Cyan
} else {
    $content = $content.Replace($before, $after)
    Set-Content -Path $path -Value $content -NoNewline
    Write-Host "OK: $occurrences llamada(s) interna(s) corregida(s)." -ForegroundColor Green
}

Write-Host "`ndotnet build para confirmar que ya compila del todo." -ForegroundColor Green