# Fix-ProductionDashboardAccess.ps1
#
# Opcion A: Production puede llegar a Dashboard para cambiar el
# festival/edicion activo desde ahi (el unico sitio donde existe ese
# selector). Cambios minimos y quirurgicos:
#
#   1. MainLayout: "dashboard" se anade a las excepciones del guardia de
#      rutas (junto a "production" y "profile" que ya estaban).
#   2. DashboardController: se anade Production a los roles permitidos -
#      necesario para que Dashboard.razor cargue sus stats.
#   3. AnalyticsController: lo mismo - Dashboard.razor tambien llama a
#      este controller para el grafico de analytics. Sin esto, el
#      selector de festival funcionaria pero el grafico de la pagina
#      quedaria roto con un error visible.
#
# NINGUN cambio quita acceso a SuperAdmin/Admin - solo se anade
# "Production" a la lista existente en los 2 controllers. Nada mas se
# toca.
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
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado - revisalo a mano): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$anyFailed = $false

# ---------------------------------------------------------------------------
# 1) MainLayout: permitir /dashboard
# ---------------------------------------------------------------------------
if (-not (Patch-File -Path "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor" -Description "MainLayout: permitir /dashboard para Production" -OldString @'
        if (!relativePath.StartsWith("production") && !relativePath.StartsWith("profile"))
        {
            Navigation.NavigateTo("/production-team");
        }
'@ -NewString @'
        if (!relativePath.StartsWith("production") && !relativePath.StartsWith("profile") && !relativePath.StartsWith("dashboard"))
        {
            Navigation.NavigateTo("/production-team");
        }
'@)) { $anyFailed = $true }

# ---------------------------------------------------------------------------
# 2) DashboardController: anadir Production
# ---------------------------------------------------------------------------
if (-not (Patch-File -Path "Alakai.FestivalManager.Api/Controllers/DashboardController.cs" -Description "DashboardController: anadir Production" -OldString @'
[Authorize(Roles = "SuperAdmin,Admin")]
public class DashboardController : ControllerBase
'@ -NewString @'
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class DashboardController : ControllerBase
'@)) { $anyFailed = $true }

# ---------------------------------------------------------------------------
# 3) AnalyticsController: anadir Production (lo usa el grafico de Dashboard)
# ---------------------------------------------------------------------------
if (-not (Patch-File -Path "Alakai.FestivalManager.Api/Controllers/AnalyticsController.cs" -Description "AnalyticsController: anadir Production" -OldString @'
[Authorize(Roles = "SuperAdmin,Admin")]
public class AnalyticsController : ControllerBase
'@ -NewString @'
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class AnalyticsController : ControllerBase
'@)) { $anyFailed = $true }

if ($anyFailed) {
    Write-Host "`nAlgun anchor no encontro su sitio exacto (puede que el atributo actual tenga otro formato en tu copia real). Pegame el contenido de las 2-3 primeras lineas de esos archivos y lo ajusto sin tocar nada mas." -ForegroundColor Red
}
else {
    Write-Host "`nProduction puede llegar a Dashboard y usar el selector de festival/edicion. SuperAdmin y Admin siguen exactamente igual que antes." -ForegroundColor Green
}