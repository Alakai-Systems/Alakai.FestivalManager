<#
    Fix-RefundDoesNotReopenPending.ps1
    -----------------------------------------------

    Cambia el comportamiento de los reembolsos: un reembolso ya NO reabre
    el registro para volver a cobrar. Hasta ahora, al reembolsar (parcial o
    totalmente), el backend forzaba PaymentStatus a "Refunded" o
    "PartiallyPaid" segun el caso, lo que hacia reaparecer un importe
    "pendiente" y el boton de pago en el panel de usuario -- justo lo
    contrario de lo que se pretende al devolver dinero.

    Con este cambio, el reembolso deja PaymentStatus tal cual estaba antes:

      - Si el registro ya estaba "Paid" (pago completo, o un plan Split ya
        completado), se queda "Paid": nada pendiente, sin boton de pago,
        aunque se reembolse parcial o totalmente.
      - Si estaba "PartiallyPaid" (plan Split con solo el primer 50%
        pagado), se queda "PartiallyPaid": el segundo 50% sigue pendiente
        de pago exactamente igual, se reembolse o no ese primer tramo --
        es un tramo aparte, no depende del reembolso.

    Esto revierte el "+RefundedAmount" que se sumaba antes al calculo de
    "pendiente" en 3 sitios del panel de usuario (la seccion de pago, la
    mini-card del dashboard, y el importe real que se manda a la pasarela
    al pulsar "Pay Remaining"), que era lo que reabria el pendiente tras un
    reembolso. El aviso de "X reembolsado" se mantiene (informativo) y
    ahora se muestra siempre que haya algo reembolsado, este el registro
    pagado del todo o no.

    Verificado contra los archivos reales del repo, con todo lo anterior
    (incluyendo Fix-DynamicCurrency.ps1 y Fix-PaymentSettingsCurrencyPlacement.ps1)
    ya aplicado. Anchors unicos, balance de llaves/parentesis limpio, diff
    revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-RefundDoesNotReopenPending.ps1

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

# 1. Alakai.FestivalManager.Application/Features/Payments/Services/PaymentService.cs -- PaymentService.cs: el reembolso ya no cambia PaymentStatus -- no reabre el registro para volver a cobrar
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Payments/Services/PaymentService.cs' `
    -Description 'PaymentService.cs: el reembolso ya no cambia PaymentStatus -- no reabre el registro para volver a cobrar' `
    -Anchor @'
        registration.RefundedAmount = alreadyRefunded + command.Amount;
        registration.PaymentStatus = registration.RefundedAmount >= registration.AmountPaid ? PaymentStatus.Refunded : PaymentStatus.PartiallyPaid;
'@ `
    -Replacement @'
        registration.RefundedAmount = alreadyRefunded + command.Amount;

        // Un reembolso NO debe reabrir el registro para volver a cobrar: es dinero
        // que se devuelve a proposito, no una deuda pendiente. Por eso ya NO se toca
        // PaymentStatus aqui -- se deja tal cual estaba antes del reembolso:
        //   - Si estaba "Paid" (pago completo, o un Split ya completado), se queda
        //     "Paid": no aparece nada pendiente ni el boton de pago.
        //   - Si estaba "PartiallyPaid" (Split con solo el primer 50% pagado), se
        //     queda "PartiallyPaid": el segundo 50% sigue pendiente de pago, se
        //     reembolse o no ese primer tramo -- es un tramo aparte.
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: la seccion de pago ya no suma RefundedAmount al pendiente; el aviso de reembolso se muestra siempre (pagado o no)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: la seccion de pago ya no suma RefundedAmount al pendiente; el aviso de reembolso se muestra siempre (pagado o no)' `
    -Anchor @'
                <div class="text-right">
                    @if (Dashboard?.Registration?.PaymentStatus == "PartiallyPaid")
                    {
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") @CurrencySymbol @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount).ToString("0.00")) @CurrencySymbol @T.Get("up_pending")</MudText>
                        @if (Dashboard.Registration.RefundedAmount > 0)
                        {
                            <MudText Class="text-xs text-danger">@Dashboard.Registration.RefundedAmount.ToString("0.00") @CurrencySymbol refunded</MudText>
                        }
                    }
                    else
                    {
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") @CurrencySymbol</MudText>
                        <MudText Class="text-warning">@PaymentStatus</MudText>
                    }
                </div>
'@ `
    -Replacement @'
                <div class="text-right">
                    @if (Dashboard?.Registration?.PaymentStatus == "PartiallyPaid")
                    {
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") @CurrencySymbol @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid).ToString("0.00")) @CurrencySymbol @T.Get("up_pending")</MudText>
                    }
                    else
                    {
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") @CurrencySymbol</MudText>
                        <MudText Class="text-warning">@PaymentStatus</MudText>
                    }
                    @if (Dashboard?.Registration?.RefundedAmount > 0)
                    {
                        <MudText Class="text-xs text-danger">@Dashboard.Registration.RefundedAmount.ToString("0.00") @CurrencySymbol refunded</MudText>
                    }
                </div>
'@

# 3. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: mini-card del dashboard ya no suma RefundedAmount al pendiente; el aviso de reembolso se muestra siempre
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: mini-card del dashboard ya no suma RefundedAmount al pendiente; el aviso de reembolso se muestra siempre' `
    -Anchor @'
    private string? PaymentCardSuffix
    {
        get
        {
            if (FinalPrice <= 0) return null;
            if (Dashboard?.Registration?.PaymentStatus == "PartiallyPaid")
            {
                decimal paid = Dashboard.Registration.AmountPaid;
                decimal refunded = Dashboard.Registration.RefundedAmount;

                // AmountPaid nunca baja al reembolsar (se guarda aparte en
                // RefundedAmount, ver PayNowAsync mas abajo para el porque), asi
                // que lo pendiente real es FinalPrice - paid + refunded. Sin el
                // +refunded esta card se quedaba con el pendiente de antes del
                // reembolso.
                string suffix = $"{paid:0.00} {CurrencySymbol} paid · {(FinalPrice - paid + refunded):0.00} {CurrencySymbol} pending";

                if (refunded > 0)
                {
                    suffix += $" · {refunded:0.00} {CurrencySymbol} refunded";
                }

                return suffix;
            }
            return $"{FinalPrice:0.00} {CurrencySymbol}";
        }
    }
'@ `
    -Replacement @'
    private string? PaymentCardSuffix
    {
        get
        {
            if (FinalPrice <= 0) return null;

            decimal refunded = Dashboard?.Registration?.RefundedAmount ?? 0;

            // Un reembolso no reabre lo pendiente (ver PayNowAsync mas abajo): si el
            // registro ya estaba "Paid" (o un Split ya completado), reembolsar no debe
            // volver a pedir ese dinero. Solo un Split con el primer 50% pagado
            // (PaymentStatus "PartiallyPaid") tiene de verdad un segundo tramo
            // pendiente, y eso no cambia si se reembolsa el primer tramo o no.
            string suffix = Dashboard?.Registration?.PaymentStatus == "PartiallyPaid"
                ? $"{Dashboard.Registration.AmountPaid:0.00} {CurrencySymbol} paid · {(FinalPrice - Dashboard.Registration.AmountPaid):0.00} {CurrencySymbol} pending"
                : $"{FinalPrice:0.00} {CurrencySymbol}";

            if (refunded > 0)
            {
                suffix += $" · {refunded:0.00} {CurrencySymbol} refunded";
            }

            return suffix;
        }
    }
'@

# 4. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: PayNowAsync ya no suma RefundedAmount al importe a cobrar -- un reembolso no vuelve a cobrarse
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: PayNowAsync ya no suma RefundedAmount al importe a cobrar -- un reembolso no vuelve a cobrarse' `
    -Anchor @'
            // Igual que en el formulario publico (Register.razor): el 1% de comision
            // se aplica SIEMPRE sobre lo que se cobra ahora, sea el importe completo
            // o solo el tramo pendiente (si ya se pago una parte). Antes solo se
            // aplicaba cuando PaymentStatus era "PartiallyPaid", asi que el pago
            // completo o el primer tramo de un Split se cobraban sin comision.
            // AmountPaid nunca se reduce al hacer un reembolso (Fix-RedsysRefunds.ps1 lo
            // guarda aparte en RefundedAmount a proposito, para no perder el historico de
            // lo cobrado). Por eso lo pendiente de verdad es FinalPrice menos lo que
            // TODAVIA se retiene (AmountPaid - RefundedAmount), o lo que es lo mismo:
            // FinalPrice - AmountPaid + RefundedAmount. Sin el +RefundedAmount, tras un
            // reembolso esto da 0 y ese 0 es justo lo que se manda a Redsys.
            decimal remaining = Dashboard.Registration.FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount;
'@ `
    -Replacement @'
            // Igual que en el formulario publico (Register.razor): el 1% de comision
            // se aplica SIEMPRE sobre lo que se cobra ahora, sea el importe completo
            // o solo el tramo pendiente (si ya se pago una parte). Antes solo se
            // aplicaba cuando PaymentStatus era "PartiallyPaid", asi que el pago
            // completo o el primer tramo de un Split se cobraban sin comision.
            // AmountPaid nunca se reduce al hacer un reembolso (se guarda aparte en
            // RefundedAmount, para no perder el historico de lo cobrado), pero un
            // reembolso NO debe volver a cobrarse: si ya estaba todo pagado (o un
            // Split ya completado), esta pantalla ni siquiera muestra el boton de pago
            // tras el reembolso. A este calculo solo se llega en el unico caso
            // legitimo que sigue pendiente: el segundo tramo de un Split cuyo primer
            // 50% ya se pago (se haya reembolsado ese primer tramo o no).
            decimal remaining = Dashboard.Registration.FinalPrice - Dashboard.Registration.AmountPaid;
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
Write-Host "Listo. Prueba: reembolsa (parcial o total) un registro ya pagado del" -ForegroundColor Cyan
Write-Host "todo -- no debe quedar nada pendiente ni aparecer el boton de pago." -ForegroundColor Cyan
Write-Host "Y en un Split con solo el primer 50% pagado, reembolsa ese primer" -ForegroundColor Cyan
Write-Host "tramo -- el segundo 50% debe seguir pendiente igual que antes." -ForegroundColor Cyan
Write-Host ""