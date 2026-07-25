# Fix-ProductionDashboardVisibility.ps1
#
# Dos causas reales, no una:
#
# 1. La ruta real de Dashboard.razor es "/" (raiz), no "/dashboard". Mi
#    guardia en MainLayout comprobaba relativePath.StartsWith("dashboard"),
#    pero Navigation.ToBaseRelativePath("/") devuelve una cadena VACIA, que
#    nunca empieza por "dashboard" - asi que aunque Production llegara a
#    esa pagina, el guardia lo seguia rebotando a Artists & Team.
#
# 2. El menu exclusivo de Production nunca tuvo un enlace a Dashboard -
#    aunque el guardia se arregle, no hay forma de navegar hasta ahi sin
#    escribir la URL a mano.
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

# ---------------------------------------------------------------------------
# 1) MainLayout: permitir la ruta raiz "" (Dashboard es "/")
# ---------------------------------------------------------------------------
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor" -Description "MainLayout: permitir la raiz (Dashboard = '/')" -OldString @'
        if (!relativePath.StartsWith("production") && !relativePath.StartsWith("profile") && !relativePath.StartsWith("dashboard"))
'@ -NewString @'
        if (!relativePath.StartsWith("production") && !relativePath.StartsWith("profile") && !string.IsNullOrEmpty(relativePath))
'@
if ($results -contains $false) { Write-Host "`nFallo (MainLayout)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 2) Sidebar: anadir Dashboard como primer item del menu exclusivo
# ---------------------------------------------------------------------------
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Layout/Sidebar.razor" -Description "Sidebar: enlace a Dashboard en el menu de Production" -OldString @'
                <ul class="relative flex flex-col gap-1" x-data="{ activeMenu: 'production' }">
                    <li class="menu nav-item">
                        <NavLink href="/production-team" class="text-black nav-link group">
'@ -NewString @'
                <ul class="relative flex flex-col gap-1" x-data="{ activeMenu: 'production' }">
                    <li class="menu nav-item">
                        <NavLink href="/" Match="NavLinkMatch.All" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-dashboard-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Dashboard</span>
                            </div>
                        </NavLink>
                    </li>
                    <li class="menu nav-item">
                        <NavLink href="/production-team" class="text-black nav-link group">
'@
if ($results -contains $false) { Write-Host "`nFallo (Sidebar)." -ForegroundColor Red; exit 1 }

Write-Host "`nDashboard visible y accesible desde el menu de Production." -ForegroundColor Green
Write-Host "Nota: con esto CUALQUIER ruta que no empiece por production/profile queda bloqueada salvo la raiz exacta - si en el futuro anades otra pagina 'compartida' fuera de production-*, hay que anadirla explicitamente aqui tambien." -ForegroundColor Yellow