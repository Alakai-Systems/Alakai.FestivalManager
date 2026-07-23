# Fix-SidebarLogoFlash.ps1
#
# EFECTO SECUNDARIO CONOCIDO del fix anterior (Fix-ActiveFestivalStateDoubleRender.ps1):
# Sidebar.razor muestra el texto "ALAKAI" siempre que ActiveFestivalState.Active?.LogoUrl
# este vacio. Como ahora ni Topbar ni Dashboard inicializan ActiveFestivalState durante
# el prerender (para evitar las cargas duplicadas), el Sidebar muestra "ALAKAI"
# garantizado durante ese instante, en vez del logo del festival activo, hasta que el
# circuito interactivo conecta y lo resuelve.
#
# Fix (minimo, sin tocar logica ni timing): en vez del texto "ALAKAI", un placeholder
# en blanco del mismo tamano, para que no haya salto visual brusco. No cambia
# ActiveFestivalState, no cambia cuando se resuelve el logo real, solo el fallback
# visual mientras tanto.
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
$results = @()
$sidebarPath = "Alakai.FestivalManager.Admin/Components/Layout/Sidebar.razor"
$results += Patch-File -Path $sidebarPath -Description "Sidebar.razor: placeholder en blanco en vez del texto ALAKAI mientras carga" -OldString @'
                else
                {
                    <span class="block mx-auto text-xl font-bold tracking-wide text-center text-black dark:text-white group-data-[sidebar=brand]/item:text-white">ALAKAI</span>
                }
'@ -NewString @'
                else
                {
                    <span class="block mx-auto h-10 w-[160px]" aria-hidden="true"></span>
                }
'@
if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Sidebar.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}
Write-Host "`nCorregido - el sidebar ya no muestra el texto ALAKAI mientras se resuelve el logo real. dotnet build para confirmar." -ForegroundColor Green