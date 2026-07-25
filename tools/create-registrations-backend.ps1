# Fix-ProductionProfileAccessAndSettingsIcon.ps1
#
# 1. MainLayout: el guardia de rutas para Production redirigia CUALQUIER
#    ruta que no empezara por "production" a /production-team - incluido
#    /profile, que si deben poder usar. Se anade como excepcion.
# 2. Topbar: el icono de engranaje (Settings) se oculta para el rol
#    Production - de todas formas el guardia les rebotaria si lo pulsaran,
#    asi que no tiene sentido mostrarlo.
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
# 1) MainLayout: permitir /profile
# ---------------------------------------------------------------------------
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor" -Description "MainLayout: permitir /profile para Production" -OldString @'
        if (!relativePath.StartsWith("production"))
        {
            Navigation.NavigateTo("/production-team");
        }
'@ -NewString @'
        if (!relativePath.StartsWith("production") && !relativePath.StartsWith("profile"))
        {
            Navigation.NavigateTo("/production-team");
        }
'@
if ($results -contains $false) { Write-Host "`nFallo (MainLayout)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 2) Topbar: ocultar el icono de Settings para Production
# ---------------------------------------------------------------------------
$topbarPath = "Alakai.FestivalManager.Admin/Components/Layout/Topbar.razor"

$results += Patch-File -Path $topbarPath -Description "Topbar: ocultar icono de Settings con @if" -OldString @'
        <a href="/settings" class="text-black dark:text-white/80">
            <i class="ri-settings-3-line text-xl leading-none"></i>
        </a>
'@ -NewString @'
        @if (!isProductionUser)
        {
            <a href="/settings" class="text-black dark:text-white/80">
                <i class="ri-settings-3-line text-xl leading-none"></i>
            </a>
        }
'@
if ($results -contains $false) { Write-Host "`nFallo (Topbar icono)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $topbarPath -Description "Topbar: campo isProductionUser" -OldString @'
    private string DisplayName { get; set; } = "Admin";
    private Guid _currentUserId;
'@ -NewString @'
    private string DisplayName { get; set; } = "Admin";
    private Guid _currentUserId;
    private bool isProductionUser;
'@
if ($results -contains $false) { Write-Host "`nFallo (Topbar campo)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $topbarPath -Description "Topbar: calcular isProductionUser" -OldString @'
            AuthenticationState authState = await AuthenticationStateTask;
            string? email = authState.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string? idClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
'@ -NewString @'
            AuthenticationState authState = await AuthenticationStateTask;
            string? email = authState.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string? idClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            isProductionUser = authState.User.IsInRole("Production");
'@
if ($results -contains $false) { Write-Host "`nFallo (Topbar calculo)." -ForegroundColor Red; exit 1 }

Write-Host "`nProfile accesible para Production, e icono de Settings oculto para ese rol." -ForegroundColor Green