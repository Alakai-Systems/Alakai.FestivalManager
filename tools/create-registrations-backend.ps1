<#
    Fix-PaymentReturnUrlsAndCardLabels.ps1
    -----------------------------------------------

    Dos arreglos independientes:

    1) BUG CRITICO: "Pay Now" desde el panel del usuario (pagos pendientes /
       segundo plazo de un Split) no mandaba UrlOk/UrlKo al crear la sesion
       de pago. Redsys lo disimulaba con un fallback en appsettings, pero
       Stripe no tiene ese fallback y responde "Missing return URLs for the
       Stripe checkout session" -- el pago ni arranca. Se construyen ahora
       igual que en el formulario publico (Register.razor), apuntando de
       vuelta al dashboard. De paso, el aviso que se mostraba al volver del
       pago (cuando no llegan parametros firmados en la URL) dejaba de
       llamar a Redsys "culpable" cuando en realidad la pasarela usada era
       Stripe.

    2) Coherencia visual: los titulos de las 3 cards del modal New/Edit
       Festival (Modules, Payment Plans, Payment Platforms) se quedaron en
       negro (text-black/70, negrita) tras el fix anterior de las cards,
       mientras el resto de labels del mismo formulario van en gris
       (text-black/60, sin negrita). Se igualan al resto para que el
       formulario sea coherente.

    Verificado contra los archivos reales del repo, con las 10 correcciones
    anteriores ya aplicadas. Anchors unicos, balance de llaves/parentesis
    limpio, diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-PaymentReturnUrlsAndCardLabels.ps1

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

# 1. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Create modal): label 'Modules' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Create modal): label ''Modules'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Create modal): label 'Payment Plans' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Create modal): label ''Payment Plans'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowFullOnline" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowFullOnline" />
'@

# 3. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Create modal): label 'Payment Platforms' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Create modal): label ''Payment Platforms'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowRedsysPlatform" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowRedsysPlatform" />
'@

# 4. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Edit modal): label 'Modules' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Edit modal): label ''Modules'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
'@

# 5. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Edit modal): label 'Payment Plans' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Edit modal): label ''Payment Plans'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowFullOnline" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowFullOnline" />
'@

# 6. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Edit modal): label 'Payment Platforms' vuelve al gris estandar del formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Edit modal): label ''Payment Platforms'' vuelve al gris estandar del formulario' `
    -Anchor @'
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowRedsysPlatform" />
'@ `
    -Replacement @'
                        <label class="block text-sm text-black/60 dark:text-white/60">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowRedsysPlatform" />
'@

# 7. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: PayNowAsync manda UrlOk/UrlKo (arregla 'Missing return URLs' de Stripe) y marca la plataforma usada
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: PayNowAsync manda UrlOk/UrlKo (arregla ''Missing return URLs'' de Stripe) y marca la plataforma usada' `
    -Anchor @'
            decimal remaining = Dashboard.Registration.FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount;
            decimal amountOverride = Math.Round(remaining * 1.01m, 2, MidpointRounding.AwayFromZero);
            CreatePaymentSessionRequest request = new() { RegistrationId = Dashboard.Registration.Id, AmountOverride = amountOverride };

            if (SelectedPaymentPlatform == "Stripe")
'@ `
    -Replacement @'
            decimal remaining = Dashboard.Registration.FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount;
            decimal amountOverride = Math.Round(remaining * 1.01m, 2, MidpointRounding.AwayFromZero);

            // Redsys tenia un fallback en appsettings si no se mandaban UrlOk/UrlKo,
            // pero Stripe no lo tiene -- sin esto, Stripe respondia "Missing return
            // URLs for the Stripe checkout session" y el pago ni arrancaba. Se
            // construyen igual que en el formulario publico (Register.razor),
            // apuntando de vuelta a este mismo dashboard.
            string baseUrl = Navigation.BaseUri.TrimEnd('/');
            string langSegment = string.IsNullOrWhiteSpace(Lang) ? string.Empty : $"/{Lang}";
            string platformSegment = SelectedPaymentPlatform.ToLowerInvariant();
            CreatePaymentSessionRequest request = new()
            {
                RegistrationId = Dashboard.Registration.Id,
                AmountOverride = amountOverride,
                UrlOk = $"{baseUrl}/user-panel/dashboard{langSegment}?payment=ok&platform={platformSegment}",
                UrlKo = $"{baseUrl}/user-panel/dashboard{langSegment}?payment=ko&platform={platformSegment}"
            };

            if (SelectedPaymentPlatform == "Stripe")
'@

# 8. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: el aviso de retorno de pago ya no culpa a Redsys cuando la pasarela usada fue Stripe
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: el aviso de retorno de pago ya no culpa a Redsys cuando la pasarela usada fue Stripe' `
    -Anchor @'
        if (query.Contains("payment=ok", StringComparison.OrdinalIgnoreCase))
        {
            ShowPaymentBanner("Payment completed at the gateway, but Redsys did not send the confirmation parameters back. Enable 'send parameters in the response URLs' on the terminal.", isError: true);
        }
        else if (query.Contains("payment=ko", StringComparison.OrdinalIgnoreCase))
'@ `
    -Replacement @'
        if (query.Contains("payment=ok", StringComparison.OrdinalIgnoreCase))
        {
            string returnPlatform = GetQueryValue(query, "platform");

            if (string.Equals(returnPlatform, "stripe", StringComparison.OrdinalIgnoreCase))
            {
                ShowPaymentBanner("Payment completed at the gateway. Confirming with Stripe, this page will refresh automatically in a few seconds.", isError: false);
            }
            else
            {
                ShowPaymentBanner("Payment completed at the gateway, but Redsys did not send the confirmation parameters back. Enable 'send parameters in the response URLs' on the terminal.", isError: true);
            }
        }
        else if (query.Contains("payment=ko", StringComparison.OrdinalIgnoreCase))
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
Write-Host "Listo. Prueba un pago pendiente (segundo plazo / Split) con Stripe desde" -ForegroundColor Cyan
Write-Host "el panel del usuario, y revisa el modal New/Edit Festival." -ForegroundColor Cyan
Write-Host ""