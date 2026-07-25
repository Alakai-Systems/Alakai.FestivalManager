# Fix-EmailImagesForceResponsive-v2.ps1
#
# La v1 tenia un fallo de sintaxis PowerShell: usaba comillas simples con
# `n para los saltos de linea, pero los backticks solo se interpretan
# dentro de comillas dobles - en comillas simples se quedan como texto
# literal "`n" en vez de convertirse en salto de linea real, por eso el
# ancla nunca coincidia con el archivo (que si tiene saltos de linea
# reales). Corregido usando bloques @'...'@ como el resto de scripts.
#
# Mismo fix que la v1: anade una regla CSS global (fuera del media query)
# que fuerza cualquier imagen dentro del email a max-width:100%, sin
# importar que atributos width/height traiga el <img> - arregla tanto el
# HTML ya guardado en EmailLayout como cualquier imagen futura.
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

$path = "Alakai.FestivalManager.Application/Features/Emails/Services/EmailNotificationService.cs"

$result = Patch-File -Path $path -Description "Regla global: forzar imagenes responsive en el email" -OldString @'
          <style>
            body {{ margin:0; padding:0; }}
            @media only screen and (max-width: 600px) {{
'@ -NewString @'
          <style>
            body {{ margin:0; padding:0; }}
            .email-shell img {{ max-width:100% !important; height:auto !important; }}
            @media only screen and (max-width: 600px) {{
'@

if (-not $result) { Write-Host "`nFallo." -ForegroundColor Red; exit 1 }

Write-Host "`nNo hace falta recrear ninguna plantilla ni volver a subir las imagenes del header/footer - este cambio arregla el HTML que YA esta guardado en cada EmailLayout, ademas de cualquier imagen que se inserte de ahora en adelante." -ForegroundColor Green