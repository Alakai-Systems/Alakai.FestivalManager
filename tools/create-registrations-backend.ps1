<#
    Fix-EditionsModalGrouping.ps1
    -------------------------------
    Sigue a Fix-EditionSchedule.ps1 (aplicalo DESPUES de ese, y despues de
    Fix-ScheduleButtonText.ps1 si ya lo has aplicado -- este no toca lo
    mismo que ese, no hay conflicto en el orden).

    Reordena visualmente la parte de abajo del modal "Edit Edition"
    (Early Bird, Schedule PDF, Active), que ahora mismo queda todo
    amalgamado sin separacion. Mismo patron que ya usas en el modal de
    Competitions.razor (los recuadros de "Levels" / "Capacities"):

      - "Early Bird" pasa a ser un recuadro con borde redondeado
        (border rounded-lg border-black/10 p-4) con su propio titulo,
        agrupando el campo de capacidad y la linea de "Used: X / Y" +
        "Reset to 0" que antes iban sueltos.
      - "Schedule PDF" pasa a ser otro recuadro igual, con su titulo,
        agrupando el enlace al PDF actual + "Remove" + el selector de
        archivo.
      - "Active" se separa con una linea superior (border-t) en vez de
        quedar pegado justo debajo del selector de archivo.

    No cambia ningun comportamiento, solo la agrupacion visual.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh ./Fix-EditionsModalGrouping.ps1

    Idempotente y todo-o-nada: si el anchor no encaja porque el archivo
    local difiere de lo esperado (p.ej. porque aun no has aplicado
    Fix-EditionSchedule.ps1), no escribe nada y lista el problema.
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
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- lo mas probable es que aun no hayas aplicado Fix-EditionSchedule.ps1). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
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

# ============================================================================
# Editions.razor: agrupar Early Bird / Schedule PDF en recuadros, separar Active
# ============================================================================
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Editions.razor' `
    -Description 'Modal Edit Edition: recuadros para Early Bird / Schedule PDF, separador para Active' `
    -Anchor @'
                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60">Early Bird capacity (leave empty to disable)</label>
                        <input class="form-input" placeholder="e.g. 30" type="number" min="0" @bind="updateRequest.EarlyBirdCapacity" />
                    </div>

                    @if (selectedEdition is not null)
                    {
                        <div class="md:col-span-2 flex items-center justify-between gap-3">
                            <span class="text-sm text-black/70 dark:text-white/70">
                                Early Bird used: <strong>@selectedEdition.EarlyBirdUsedCount</strong>@(updateRequest.EarlyBirdCapacity.HasValue ? $" / {updateRequest.EarlyBirdCapacity}" : "")
                            </span>
                            <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="ResetEarlyBirdUsageAsync">
                                Reset to 0
                            </button>
                        </div>
                    }

                    @if (selectedEdition is not null)
                    {
                        <div class="md:col-span-2 flex flex-col gap-2">
                            <label class="block text-sm text-black/60 dark:text-white/60">Schedule PDF</label>
                            @if (!string.IsNullOrWhiteSpace(selectedEdition.ScheduleUrl))
                            {
                                <div class="flex items-center justify-between gap-3">
                                    <a href="@selectedEdition.ScheduleUrl" target="_blank" class="btn border border-purple text-purple hover:bg-purple hover:text-white inline-flex items-center gap-1">
                                        <i class="ri-file-pdf-line"></i>View current PDF
                                    </a>
                                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="RemoveScheduleAsync">
                                        Remove
                                    </button>
                                </div>
                            }
                            <InputFile OnChange="OnScheduleSelected" accept="application/pdf" disabled="@isSaving" />
                        </div>
                    }

                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white md:col-span-2">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
'@ `
    -Replacement @'
                    @if (selectedEdition is not null)
                    {
                        <div class="md:col-span-2 p-4 border rounded-lg border-black/10 dark:border-darkborder">
                            <h4 class="mb-3 text-sm font-semibold text-black dark:text-white">Early Bird</h4>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Capacity (leave empty to disable)</label>
                                <input class="form-input" placeholder="e.g. 30" type="number" min="0" @bind="updateRequest.EarlyBirdCapacity" />
                            </div>

                            <div class="flex items-center justify-between gap-3 mt-3">
                                <span class="text-sm text-black/70 dark:text-white/70">
                                    Used: <strong>@selectedEdition.EarlyBirdUsedCount</strong>@(updateRequest.EarlyBirdCapacity.HasValue ? $" / {updateRequest.EarlyBirdCapacity}" : "")
                                </span>
                                <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="ResetEarlyBirdUsageAsync">
                                    Reset to 0
                                </button>
                            </div>
                        </div>

                        <div class="md:col-span-2 p-4 border rounded-lg border-black/10 dark:border-darkborder">
                            <h4 class="mb-3 text-sm font-semibold text-black dark:text-white">Schedule PDF</h4>

                            @if (!string.IsNullOrWhiteSpace(selectedEdition.ScheduleUrl))
                            {
                                <div class="flex items-center justify-between gap-3 mb-3">
                                    <a href="@selectedEdition.ScheduleUrl" target="_blank" class="btn border border-purple text-purple hover:bg-purple hover:text-white inline-flex items-center gap-1">
                                        <i class="ri-file-pdf-line"></i>View current PDF
                                    </a>
                                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="RemoveScheduleAsync">
                                        Remove
                                    </button>
                                </div>
                            }
                            <InputFile OnChange="OnScheduleSelected" accept="application/pdf" disabled="@isSaving" />
                        </div>
                    }

                    <div class="md:col-span-2 pt-3 border-t border-black/10 dark:border-darkborder">
                        <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                            <input type="checkbox" @bind="updateRequest.IsActive" />
                            Active
                        </label>
                    </div>
'@

# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = Test-PatchOperation -Op $op
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
    Write-Host "Si el problema es que no encuentra el anchor, aplica primero" -ForegroundColor Yellow
    Write-Host "Fix-EditionSchedule.ps1 y luego reintenta este." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Listo. Solo CSS/markup, no hace falta migracion ni build especial." -ForegroundColor Cyan
Write-Host "En Admin > Editions > Edit deberias ver 'Early Bird' y 'Schedule PDF'" -ForegroundColor Cyan
Write-Host "cada uno en su propio recuadro con borde, y 'Active' separado por una" -ForegroundColor Cyan
Write-Host "linea encima." -ForegroundColor Cyan
Write-Host ""