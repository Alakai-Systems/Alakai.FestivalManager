<#
.SINOPSIS
  Elimina la pagina "Check-in" del panel de Admin (la que aparece en el menu
  Operations > Check-in, /checkin) y su enlace en el menu lateral. No toca
  nada de la Api (TicketsController, PublicCheckInController, el check-in
  publico por QR) ni ningun otro fichero - solo esta pagina de Admin y su
  entrada de menu, que es exactamente lo que se ha pedido quitar.

  Archivos que modifica:
    - Admin\Components\Layout\Sidebar.razor   (quita el <li> de Check-in)
    - Admin\Components\Pages\CheckIn.razor    (se elimina el fichero)

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\31-remove-checkin-admin-page.ps1

  Luego: dotnet build (Admin, debe compilar limpio), redeploy del App Service
  de Admin.
#>

function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$SidebarPath = ".\Alakai.FestivalManager.Admin\Components\Layout\Sidebar.razor"
$CheckInPagePath = ".\Alakai.FestivalManager.Admin\Components\Pages\CheckIn.razor"

# ----------------------------------------------------------------------------
# 1. Quitar el enlace "Check-in" del menu lateral
# ----------------------------------------------------------------------------
Patch-File `
    -Path $SidebarPath `
    -Description "quitar enlace Check-in del menu" `
    -OldString @'
                        <li><NavLink href="/registrations">Registrations</NavLink></li>
                        <li><NavLink href="/checkin">Check-in</NavLink></li>
                        <li><NavLink href="/competition-entries">Competition Entries</NavLink></li>
'@ `
    -NewString @'
                        <li><NavLink href="/registrations">Registrations</NavLink></li>
                        <li><NavLink href="/competition-entries">Competition Entries</NavLink></li>
'@

# ----------------------------------------------------------------------------
# 2. Eliminar el fichero de la pagina Check-in
# ----------------------------------------------------------------------------
if (Test-Path -LiteralPath $CheckInPagePath) {
    Remove-Item -LiteralPath $CheckInPagePath -Force
    Write-Host "OK: eliminado -> $CheckInPagePath" -ForegroundColor Green
} else {
    Write-Host "SKIP: ya no existe -> $CheckInPagePath" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "Deberias ver 2 lineas 'OK'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO: dotnet build (Admin) y redeploy del App Service de Admin." -ForegroundColor Cyan