<#
    Fix-EditionsModalLayoutV2.ps1
    ------------------------------
    Corrige el layout en 2 columnas de los modales "New Edition" / "Edit
    Edition" (Admin > Editions), que quedo roto tras Fix-EditionsModalLayout.ps1
    (inventado desde cero, con max-w-2xl y una combinacion de clases -w-full,
    mb-1, gap-x-4/gap-y-4- que no es la que usa el resto del Admin).

    En vez de inventar de nuevo, esta version copia EXACTAMENTE el patron que
    ya usan los modales de 2 columnas que funcionan bien en esta misma app
    (Festivals.razor, "New/Edit Festival"):

      - El modal se queda en max-w-lg (el ancho no cambia; el 2a columna sale
        del grid de dentro, no de ensanchar el modal).
      - El grid es "grid grid-cols-1 gap-4 p-5 md:grid-cols-2" (un solo gap,
        no gap-x/gap-y por separado).
      - Los campos sin etiqueta (Name, Year) van sueltos como hijos directos
        del grid, con su placeholder, igual que en Festivals. No hace falta
        w-full: .form-input/.form-select ya ocupan el 100% del ancho.
      - Los campos con etiqueta (fechas, Early Bird capacity, Festival en
        Edit) van en un <div> con <label class="block text-sm text-black/60
        dark:text-white/60"> (sin mb-1) encima, exactamente como en
        Festivals.razor.
      - Los que ocupan las dos columnas llevan la clase md:col-span-2
        directamente.
      - El checkbox "Active" va al final del grid como fila completa, con
        md:col-span-2 en la propia etiqueta -- calcado del checkbox "Active"
        de Festivals.razor.

    Requiere tener aplicados Fix-EarlyBirdPricing.ps1 y
    Fix-EditionsModalLayout.ps1 (el de antes). Si tu Editions.razor no tiene
    ya el layout de 2 columnas roto, este script no encontrara los anchors y
    no tocara nada (todo o nada).

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh ./Fix-EditionsModalLayoutV2.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

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
        return "Anchor no encontrado en $($Op.Path) (probablemente falta aplicar Fix-EditionsModalLayout.ps1 antes, o el archivo local difiere de lo esperado). Descripcion: $($Op.Description)"
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
# 1) New Edition: mismo patron de grid que Festivals.razor (max-w-lg)
# ============================================================================
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Editions.razor' `
    -Description 'New Edition: layout igual que Festivals.razor' `
    -Anchor @'
            <div class="relative w-full max-w-2xl overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <!-- New Edition modal input area -->
                <div class="p-5 grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="md:col-span-2 p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Festival</label>
                        <select class="form-select w-full" @bind="createRequest.FestivalId">
                            <option value="@Guid.Empty">Select festival</option>
                            @foreach (FestivalDto festival in festivals)
                            {
                                <option value="@festival.Id">@festival.Name</option>
                            }
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Name</label>
                        <input class="form-input w-full" placeholder="Name" @bind="createRequest.Name" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Year</label>
                        <input class="form-input w-full" placeholder="Year" type="number" @bind="createRequest.Year" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Start date</label>
                        <InputDate class="form-input w-full" @bind-Value="createRequest.StartDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">End date</label>
                        <InputDate class="form-input w-full" @bind-Value="createRequest.EndDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Registration opens</label>
                        <InputDate class="form-input w-full" @bind-Value="createRequest.RegistrationOpenDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Registration closes</label>
                        <InputDate class="form-input w-full" @bind-Value="createRequest.RegistrationCloseDate" />
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Early Bird capacity (leave empty to disable)</label>
                        <input class="form-input w-full" placeholder="e.g. 30" type="number" min="0" @bind="createRequest.EarlyBirdCapacity" />
                    </div>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreateEditionAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
'@ `
    -Replacement @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded md:col-span-2 bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <select class="form-select md:col-span-2" @bind="createRequest.FestivalId">
                        <option value="@Guid.Empty">Select festival</option>
                        @foreach (FestivalDto festival in festivals)
                        {
                            <option value="@festival.Id">@festival.Name</option>
                        }
                    </select>

                    <input class="form-input" placeholder="Name" @bind="createRequest.Name" />
                    <input class="form-input" placeholder="Year" type="number" @bind="createRequest.Year" />

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Start date</label>
                        <InputDate class="form-input" @bind-Value="createRequest.StartDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">End date</label>
                        <InputDate class="form-input" @bind-Value="createRequest.EndDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Registration opens</label>
                        <InputDate class="form-input" @bind-Value="createRequest.RegistrationOpenDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Registration closes</label>
                        <InputDate class="form-input" @bind-Value="createRequest.RegistrationCloseDate" />
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60">Early Bird capacity (leave empty to disable)</label>
                        <input class="form-input" placeholder="e.g. 30" type="number" min="0" @bind="createRequest.EarlyBirdCapacity" />
                    </div>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreateEditionAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
'@

# ============================================================================
# 2) Edit Edition: mismo patron de grid que Festivals.razor (max-w-lg)
# ============================================================================
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Editions.razor' `
    -Description 'Edit Edition: layout igual que Festivals.razor' `
    -Anchor @'
            <div class="relative w-full max-w-2xl overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="md:col-span-2 p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Festival</label>
                        <select class="form-select w-full" @bind="updateRequest.FestivalId">
                            <option value="@Guid.Empty">Select festival</option>
                            @foreach (FestivalDto festival in festivals)
                            {
                                <option value="@festival.Id">@festival.Name</option>
                            }
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Name</label>
                        <input class="form-input w-full" placeholder="Name" @bind="updateRequest.Name" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Year</label>
                        <input class="form-input w-full" placeholder="Year" type="number" @bind="updateRequest.Year" />
                    </div>

                    <div class="flex items-end pb-2">
                        <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                            <input type="checkbox" @bind="updateRequest.IsActive" />
                            Active
                        </label>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Start date</label>
                        <InputDate class="form-input w-full" @bind-Value="updateRequest.StartDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">End date</label>
                        <InputDate class="form-input w-full" @bind-Value="updateRequest.EndDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Registration opens</label>
                        <InputDate class="form-input w-full" @bind-Value="updateRequest.RegistrationOpenDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Registration closes</label>
                        <InputDate class="form-input w-full" @bind-Value="updateRequest.RegistrationCloseDate" />
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Early Bird capacity (leave empty to disable)</label>
                        <input class="form-input w-full" placeholder="e.g. 30" type="number" min="0" @bind="updateRequest.EarlyBirdCapacity" />
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
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdateEditionAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
'@ `
    -Replacement @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded md:col-span-2 bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60">Festival</label>
                        <select class="form-select" @bind="updateRequest.FestivalId">
                            <option value="@Guid.Empty">Select festival</option>
                            @foreach (FestivalDto festival in festivals)
                            {
                                <option value="@festival.Id">@festival.Name</option>
                            }
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Name</label>
                        <input class="form-input" placeholder="Name" @bind="updateRequest.Name" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Year</label>
                        <input class="form-input" placeholder="Year" type="number" @bind="updateRequest.Year" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Start date</label>
                        <InputDate class="form-input" @bind-Value="updateRequest.StartDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">End date</label>
                        <InputDate class="form-input" @bind-Value="updateRequest.EndDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Registration opens</label>
                        <InputDate class="form-input" @bind-Value="updateRequest.RegistrationOpenDate" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Registration closes</label>
                        <InputDate class="form-input" @bind-Value="updateRequest.RegistrationCloseDate" />
                    </div>

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

                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white md:col-span-2">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdateEditionAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
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
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Listo. dotnet build y recarga Admin > Editions para verlo." -ForegroundColor Cyan
Write-Host ""