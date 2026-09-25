<#
    Fix-FestivalCardsLayout.ps1
    -----------------------------------------------

    Mejora visual en los modales "New Festival" y "Edit Festival": las
    secciones Modules, Payment Plans y Payment Platforms ahora van cada
    una dentro de su propia card (recuadro con borde suave y fondo
    ligeramente distinto), en vez de ir las 3 seguidas sin separacion
    visual como hasta ahora.

    No cambia ningun campo, binding ni logica -- solo el markup/estilos
    de esas 3 secciones, en los dos modales (Create y Edit).

    Verificado contra los archivos reales del repo, con las 9 correcciones
    anteriores ya aplicadas. Anchors unicos, balance de llaves/parentesis
    limpio, diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-FestivalCardsLayout.ps1

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

# 1. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Create modal): Modules/Payment Plans/Payment Platforms cada uno en su propia card (recuadro suave)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Create modal): Modules/Payment Plans/Payment Platforms cada uno en su propia card (recuadro suave)' `
    -Anchor @'
                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowFullOnline" />
                                Pay 100% now
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowSplitFiftyFifty" />
                                Split 50/50
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowDeferredTenDays" />
                                Deferred (10 days)
                            </label>
                        </div>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowRedsysPlatform" />
                                Redsys (bank)
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowStripePlatform" />
                                Stripe
                            </label>
                        </div>
                    </div>
'@ `
    -Replacement @'
                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowFullOnline" />
                                Pay 100% now
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowSplitFiftyFifty" />
                                Split 50/50
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowDeferredTenDays" />
                                Deferred (10 days)
                            </label>
                        </div>
                    </div>

                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowRedsysPlatform" />
                                Redsys (bank)
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateAllowStripePlatform" />
                                Stripe
                            </label>
                        </div>
                    </div>
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor (Edit modal): Modules/Payment Plans/Payment Platforms cada uno en su propia card (recuadro suave)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor (Edit modal): Modules/Payment Plans/Payment Platforms cada uno en su propia card (recuadro suave)' `
    -Anchor @'
                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowFullOnline" />
                                Pay 100% now
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowSplitFiftyFifty" />
                                Split 50/50
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowDeferredTenDays" />
                                Deferred (10 days)
                            </label>
                        </div>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowRedsysPlatform" />
                                Redsys (bank)
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowStripePlatform" />
                                Stripe
                            </label>
                        </div>
                    </div>
'@ `
    -Replacement @'
                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Plans</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowFullOnline" />
                                Pay 100% now
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowSplitFiftyFifty" />
                                Split 50/50
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowDeferredTenDays" />
                                Deferred (10 days)
                            </label>
                        </div>
                    </div>

                    <div class="p-4 space-y-2 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-2">
                        <label class="block text-sm font-medium text-black/70 dark:text-white/70">Payment Platforms</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowRedsysPlatform" />
                                Redsys (bank)
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateAllowStripePlatform" />
                                Stripe
                            </label>
                        </div>
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
Write-Host "Listo. Revisa el diff (git diff) y prueba abriendo New Festival y Edit" -ForegroundColor Cyan
Write-Host "Festival de un festival cualquiera." -ForegroundColor Cyan
Write-Host ""