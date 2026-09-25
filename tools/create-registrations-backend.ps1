<#
    Fix-PaymentSettingsCurrencyPlacement.ps1
    -----------------------------------------------

    Requiere Fix-DynamicCurrency.ps1 ya aplicado (y, por tanto, tambien todo
    lo anterior: Fix-PaymentSettings3Columns.ps1, Fix-StripeFee.ps1, etc.).

    Corrige el sitio y tamano de la card de Currency en el modal "Payment
    Settings" tal y como se pidio:

      - Primera fila vuelve a ser Redsys, Stripe, Email (en ese orden,
        como estaba antes) -- Currency ya no va delante de Redsys.
      - Currency pasa a su propia fila, DEBAJO de las 3 columnas, ocupando
        solo el ancho de las dos columnas de pago (Redsys + Stripe), no el
        de Email.
      - Al estar sola en su propia fila del grid, la card ya no se estira
        a la altura de sus vecinas -- el alto es solo el del selector.

    Verificado contra los archivos reales del repo, con Fix-DynamicCurrency.ps1
    (y todo lo anterior) ya aplicado. Anchors unicos, balance de llaves/
    parentesis limpio, diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-PaymentSettingsCurrencyPlacement.ps1

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

# 1. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor: quita la card de Currency de arriba (delante de Redsys) -- vuelve a dejar Redsys como primera columna
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor: quita la card de Currency de arriba (delante de Redsys) -- vuelve a dejar Redsys como primera columna' `
    -Anchor @'
                        <div class="p-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-3">
                            <div class="max-w-xs">
                                <label class="block text-sm text-black/60 dark:text-white/60">Currency (applies to Redsys &amp; Stripe)</label>
                                <select class="form-select" @bind="credentialsRequest.Currency">
                                    <option value="EUR">EUR (€)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="CAD">CAD (CA$)</option>
                                </select>
                            </div>
                        </div>

                        <div class="p-4 space-y-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black dark:text-white">Redsys</p>
'@ `
    -Replacement @'
                        <div class="p-4 space-y-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black dark:text-white">Redsys</p>
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor: anade la card de Currency al final, debajo de Redsys/Stripe/Email, ocupando solo 2 columnas (el ancho de Redsys+Stripe)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor: anade la card de Currency al final, debajo de Redsys/Stripe/Email, ocupando solo 2 columnas (el ancho de Redsys+Stripe)' `
    -Anchor @'
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="credentialsRequest.EmailUseSSL" />
                                Use SSL/TLS
                            </label>
                        </div>
                    }
                </div>
'@ `
    -Replacement @'
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="credentialsRequest.EmailUseSSL" />
                                Use SSL/TLS
                            </label>
                        </div>

                        <div class="p-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                            <div class="max-w-xs">
                                <label class="block text-sm text-black/60 dark:text-white/60">Currency (applies to Redsys &amp; Stripe)</label>
                                <select class="form-select" @bind="credentialsRequest.Currency">
                                    <option value="EUR">EUR (€)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="CAD">CAD (CA$)</option>
                                </select>
                            </div>
                        </div>
                    }
                </div>
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
Write-Host "Listo. Abre Payment Settings: primera fila Redsys / Stripe / Email," -ForegroundColor Cyan
Write-Host "y debajo, ocupando el ancho de Redsys+Stripe, una card corta con" -ForegroundColor Cyan
Write-Host "solo el selector de Currency." -ForegroundColor Cyan
Write-Host ""