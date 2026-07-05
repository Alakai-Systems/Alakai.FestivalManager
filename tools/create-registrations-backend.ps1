# ============================================================================
# patch-userpanel-module-icon-colors.ps1
# Gives the module cards (Accommodation / Bus / Meals) their own icon colors,
# distinct from the first row (purple/yellow/green/light-blue):
#   Accommodation -> indigo, Bus -> pink, Meals -> blue
#
# REQUIRES patch-userpanel-module-cards-v2.ps1 to have been applied first.
# Run from the solution root. All-or-nothing.
# ============================================================================

$ErrorActionPreference = 'Stop'

$path = 'Alakai.FestivalManager.Admin\Components\Pages\UserPanelDashboard\UserPanel.razor'

if (-not (Test-Path $path)) {
    Write-Host "ERROR: File not found: $path" -ForegroundColor Red
    Write-Host "Run this script from the solution root." -ForegroundColor Red
    exit 1
}

$content = [System.IO.File]::ReadAllText($path)
$content = $content -replace "`r`n", "`n"

$failures = @()

# ---------- Patch 1 ----------
$old1 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-purple/10 text-purple shrink-0">
                            <i class="ri-hotel-bed-line text-2xl"></i>
'@

$new1 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-indigo-400/20 text-indigo-500 shrink-0">
                            <i class="ri-hotel-bed-line text-2xl"></i>
'@

$old1 = $old1 -replace "`r`n", "`n"
$new1 = $new1 -replace "`r`n", "`n"
$c1 = ([regex]::Matches($content, [regex]::Escape($old1))).Count
if ($c1 -ne 1) { $failures += "Patch 1 target found $c1 times (expected 1). Did you run patch-userpanel-module-cards-v2.ps1 first?" }

# ---------- Patch 2 ----------
$old2 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-warning/10 text-warning shrink-0">
                            <i class="ri-bus-2-line text-2xl"></i>
'@

$new2 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-pink-500/10 text-pink-500 shrink-0">
                            <i class="ri-bus-2-line text-2xl"></i>
'@

$old2 = $old2 -replace "`r`n", "`n"
$new2 = $new2 -replace "`r`n", "`n"
$c2 = ([regex]::Matches($content, [regex]::Escape($old2))).Count
if ($c2 -ne 1) { $failures += "Patch 2 target found $c2 times (expected 1). Did you run patch-userpanel-module-cards-v2.ps1 first?" }

# ---------- Patch 3 ----------
$old3 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-success/10 text-success shrink-0">
                            <i class="ri-restaurant-line text-2xl"></i>
'@

$new3 = @'
<div class="flex items-center justify-center w-12 h-12 rounded bg-blue-500/10 text-blue-500 shrink-0">
                            <i class="ri-restaurant-line text-2xl"></i>
'@

$old3 = $old3 -replace "`r`n", "`n"
$new3 = $new3 -replace "`r`n", "`n"
$c3 = ([regex]::Matches($content, [regex]::Escape($old3))).Count
if ($c3 -ne 1) { $failures += "Patch 3 target found $c3 times (expected 1). Did you run patch-userpanel-module-cards-v2.ps1 first?" }

if ($failures.Count -gt 0) {
    Write-Host "ABORTED. Nothing was saved. Problems found:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}

$content = $content.Replace($old1, $new1)
$content = $content.Replace($old2, $new2)
$content = $content.Replace($old3, $new3)

[System.IO.File]::WriteAllText($path, $content, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "OK: module card icon colors updated (indigo / pink / blue)." -ForegroundColor Green
Write-Host "Now rebuild: dotnet build" -ForegroundColor Cyan