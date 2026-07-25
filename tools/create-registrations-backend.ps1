# Fix-ProductionSidebarIconsAndCleanup.ps1
#
# 1. Icono distinto por cada item del menu exclusivo de Production (ahora
#    todos usan ri-team-fill).
# 2. "Accommodation" / "Accommodation Buildings" -> "Accommodation Setup"
#    en las dos versiones del menu (el completo y el exclusivo).
# 3. De paso: seguian ahi "Accommodation Zones" y "Accommodation Rooms" en
#    el menu exclusivo - el fix que te di antes para quitarlos no se llego
#    a aplicar. Los quito ahora mismo.
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
    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan; return $true }
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$path = "Alakai.FestivalManager.Admin/Components/Layout/Sidebar.razor"
$results = @()

# ---------------------------------------------------------------------------
# Menu completo: renombrar "Accommodation" -> "Accommodation Setup"
# ---------------------------------------------------------------------------
$results += Patch-File -Path $path -Description "Menu completo: Accommodation -> Accommodation Setup" -OldString @'
                        <li><NavLink href="/production-buildings">Accommodation</NavLink></li>
'@ -NewString @'
                        <li><NavLink href="/production-buildings">Accommodation Setup</NavLink></li>
'@
if ($results -contains $false) { Write-Host "`nFallo (menu completo)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# Menu exclusivo: iconos distintos + rename + quitar Zones/Rooms sueltos
# ---------------------------------------------------------------------------
$results += Patch-File -Path $path -Description "Menu exclusivo: icono Artists & Team" -OldString @'
                        <NavLink href="/production-team" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Artists &amp; Team</span>
'@ -NewString @'
                        <NavLink href="/production-team" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-user-star-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Artists &amp; Team</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Artists & Team)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "Menu exclusivo: icono Suppliers" -OldString @'
                        <NavLink href="/production-suppliers" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Suppliers</span>
'@ -NewString @'
                        <NavLink href="/production-suppliers" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-truck-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Suppliers</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Suppliers)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "Menu exclusivo: icono + rename Accommodation Setup" -OldString @'
                        <NavLink href="/production-buildings" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Buildings</span>
'@ -NewString @'
                        <NavLink href="/production-buildings" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-hotel-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Setup</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Accommodation Setup)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "Menu exclusivo: quitar bloque Accommodation Zones (residuo)" -OldString @'
                    <li class="menu nav-item">
                        <NavLink href="/production-zones" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Zones</span>
                            </div>
                        </NavLink>
                    </li>
'@ -NewString @'
'@
if ($results -contains $false) { Write-Host "`nSKIP: bloque Accommodation Zones no encontrado (puede que ya no exista)." -ForegroundColor Yellow }

$results += Patch-File -Path $path -Description "Menu exclusivo: quitar bloque Accommodation Rooms (residuo)" -OldString @'
                    <li class="menu nav-item">
                        <NavLink href="/production-accommodations" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Rooms</span>
                            </div>
                        </NavLink>
                    </li>
'@ -NewString @'
'@
if ($results -contains $false) { Write-Host "`nSKIP: bloque Accommodation Rooms no encontrado (puede que ya no exista)." -ForegroundColor Yellow }

$results += Patch-File -Path $path -Description "Menu exclusivo: icono Accommodation Reservations" -OldString @'
                        <NavLink href="/production-reservations" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Reservations</span>
'@ -NewString @'
                        <NavLink href="/production-reservations" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-calendar-check-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Accommodation Reservations</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Reservations)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "Menu exclusivo: icono Trips" -OldString @'
                        <NavLink href="/production-trips" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Trips</span>
'@ -NewString @'
                        <NavLink href="/production-trips" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-flight-takeoff-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Trips</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Trips)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "Menu exclusivo: icono Runner Itineraries" -OldString @'
                        <NavLink href="/production-itineraries" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Runner Itineraries</span>
'@ -NewString @'
                        <NavLink href="/production-itineraries" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-route-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Runner Itineraries</span>
'@
if ($results -contains $false) { Write-Host "`nFallo (icono Runner Itineraries)." -ForegroundColor Red; exit 1 }

Write-Host "`nMenu de Production: iconos distintos por item, 'Accommodation Setup' en ambas versiones del menu, y los residuos de Zones/Rooms fuera de una vez." -ForegroundColor Green