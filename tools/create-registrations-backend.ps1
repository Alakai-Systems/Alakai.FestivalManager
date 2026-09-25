<#
    Fix-DashboardRefundDisplay.ps1
    -----------------------------------------------

    Tras un reembolso parcial (probado con Stripe), el panel del usuario
    no lo reflejaba bien en dos sitios:

      1) BUG: la mini-card "Payment" de arriba del dashboard calculaba lo
         pendiente como FinalPrice - AmountPaid, sin sumar RefundedAmount
         -- a diferencia del apartado de pago de mas abajo, que ya sumaba
         RefundedAmount correctamente. Con un reembolso de 27.50e sobre un
         registro de 255e (127.50 pagado, split 50/50), la mini-card se
         quedaba en "127.50 pending" en vez de los "155.00 pending"
         correctos.

      2) Ni la mini-card ni el apartado de pago mostraban en ningun sitio
         que hubiera habido un reembolso. AmountPaid no baja al reembolsar
         a proposito (queda guardado en RefundedAmount, para no perder el
         historico de lo cobrado -- ver Fix-RedsysRefunds.ps1), pero sin
         una linea que lo explique, parece que el reembolso no se ha
         registrado en ningun lado. Se anade una linea con el importe
         reembolsado en los dos sitios, con el mismo estilo (texto rojo)
         que ya se usa para esto en el listado de inscripciones del Admin.

    Verificado contra los archivos reales del repo, con las 12 correcciones
    anteriores ya aplicadas. Anchors unicos, balance de llaves/parentesis
    limpio, diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-DashboardRefundDisplay.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado, no escribe nada y lista el problema.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

# ----------------------------------------------------------------------------
# Helpers (identicos a los scripts anteriores)
# ----------------------------------------------------------------------------

function Get-NormalizedContent {
    param([string]$Path)

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")
    $normalized = $raw.Replace("`r`n", "`n")

    return [PSCustomObject]@{
        Raw        = $raw
        Normalized = $normalized
        UsesCrlf   = $usesCrlf
    }
}

function Set-NormalizedContent {
    param(
        [string]$Path,
        [string]$NormalizedContent,
        [bool]$UsesCrlf
    )

    $final = if ($UsesCrlf) { $NormalizedContent.Replace("`n", "`r`n") } else { $NormalizedContent }
    [System.IO.File]::WriteAllText($Path, $final, [System.Text.UTF8Encoding]::new($false))
}

function Convert-ToLf {
    param([string]$Text)
    return $Text.Replace("`r`n", "`n")
}

$script:PlanErrors = @()
$script:Plan = @()

function Add-PatchOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Anchor,
        [Parameter(Mandatory)][string]$Replacement,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Patch'
        Path        = $Path
        Anchor      = Convert-ToLf $Anchor
        Replacement = Convert-ToLf $Replacement
        Description = $Description
    }
}

function Add-CreateOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Create'
        Path        = $Path
        Content     = Convert-ToLf $Content
        Description = $Description
    }
}

function Test-PatchOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return "No existe el archivo: $($Op.Path)"
    }

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 1) {
        return $null
    }

    if ($anchorCount -eq 0) {
        $replacementCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Replacement))).Count
        if ($replacementCount -ge 1) {
            return $null  # ya aplicado -> idempotente
        }
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- revisalo a mano). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Test-CreateOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return $null
    }

    $existing = Get-NormalizedContent -Path $Op.Path

    if ($existing.Normalized.TrimEnd() -eq $Op.Content.TrimEnd()) {
        return $null  # ya existe con el contenido esperado -> idempotente
    }

    return "Ya existe $($Op.Path) con un contenido distinto al esperado; revisalo a mano antes de reintentar."
}

function Invoke-PatchOperation {
    param($Op)

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 0) {
        Write-Host "  = ya aplicado: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $newNormalized = $file.Normalized.Replace($Op.Anchor, $Op.Replacement)
    Set-NormalizedContent -Path $Op.Path -NormalizedContent $newNormalized -UsesCrlf $file.UsesCrlf

    Write-Host "  + patched: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

function Invoke-CreateOperation {
    param($Op)

    if (Test-Path -LiteralPath $Op.Path) {
        Write-Host "  = ya existe: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $dir = Split-Path -Parent $Op.Path
    if ($dir -and -not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    Set-NormalizedContent -Path $Op.Path -NormalizedContent $Op.Content -UsesCrlf $false

    Write-Host "  + created: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# 1. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: PaymentCardSuffix (mini-card del dashboard) suma RefundedAmount al pendiente, igual que el resto del fichero, y muestra el importe reembolsado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: PaymentCardSuffix (mini-card del dashboard) suma RefundedAmount al pendiente, igual que el resto del fichero, y muestra el importe reembolsado' `
    -Anchor @'
            if (Dashboard?.Registration?.PaymentStatus == "PartiallyPaid")
            {
                decimal paid = Dashboard.Registration.AmountPaid;
                return $"{paid:0.00} € paid · {(FinalPrice - paid):0.00} € pending";
            }
            return $"{FinalPrice:0.00} €";
'@ `
    -Replacement @'
            if (Dashboard?.Registration?.PaymentStatus == "PartiallyPaid")
            {
                decimal paid = Dashboard.Registration.AmountPaid;
                decimal refunded = Dashboard.Registration.RefundedAmount;

                // AmountPaid nunca baja al reembolsar (se guarda aparte en
                // RefundedAmount, ver PayNowAsync mas abajo para el porque), asi
                // que lo pendiente real es FinalPrice - paid + refunded. Sin el
                // +refunded esta card se quedaba con el pendiente de antes del
                // reembolso.
                string suffix = $"{paid:0.00} € paid · {(FinalPrice - paid + refunded):0.00} € pending";

                if (refunded > 0)
                {
                    suffix += $" · {refunded:0.00} € refunded";
                }

                return suffix;
            }
            return $"{FinalPrice:0.00} €";
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: el apartado de pago muestra el importe reembolsado (antes no se veia en ningun sitio, aunque el pendiente ya lo tenia en cuenta)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: el apartado de pago muestra el importe reembolsado (antes no se veia en ningun sitio, aunque el pendiente ya lo tenia en cuenta)' `
    -Anchor @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") € @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount).ToString("0.00")) € @T.Get("up_pending")</MudText>
'@ `
    -Replacement @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") € @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount).ToString("0.00")) € @T.Get("up_pending")</MudText>
                        @if (Dashboard.Registration.RefundedAmount > 0)
                        {
                            <MudText Class="text-xs text-danger">@Dashboard.Registration.RefundedAmount.ToString("0.00") € refunded</MudText>
                        }
'@


# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = if ($op.Type -eq 'Patch') { Test-PatchOperation -Op $op } else { Test-CreateOperation -Op $op }
    if ($err) {
        $script:PlanErrors += $err
    }
}

if ($script:PlanErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "NO se ha escrito nada. $($script:PlanErrors.Count) problema(s) encontrados:" -ForegroundColor Red
    foreach ($e in $script:PlanErrors) {
        Write-Host "  - $e" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Revisa esos archivos a mano, o dime que ha cambiado para regenerar el script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    if ($op.Type -eq 'Patch') {
        Invoke-PatchOperation -Op $op
    }
    else {
        Invoke-CreateOperation -Op $op
    }
}

Write-Host ""
Write-Host "Listo. Recarga el panel del usuario del registro al que le hiciste el" -ForegroundColor Cyan
Write-Host "reembolso de prueba y comprueba la mini-card y el apartado de pago." -ForegroundColor Cyan
Write-Host ""