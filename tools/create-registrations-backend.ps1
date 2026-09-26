<#
    Fix-GenerateInvoicesModalWidth.ps1
    -----------------------------------------------

    El modal "Generate Invoices" seguia saliendo a pantalla completa
    despues de reiniciar el server -- asi que no era un tema de cache ni
    de build sin recompilar. El .razor ya tenia bien puesto
    w-[92vw] md:w-[380px] (igual que el resto de modales de confirmacion),
    pero esa clase de Tailwind no esta surtiendo efecto por lo que sea en
    tu build.

    En vez de seguir investigando por que esa clase concreta de Tailwind
    no compila (que puede llevar varias rondas mas), este script rodea el
    problema: mete el mismo ancho responsive como CSS normal, en
    style.css, con su propio nombre de clase (.confirm-modal-380) --
    CSS normal no depende de que Tailwind genere nada nuevo, asi que
    funciona seguro.

    Que trae:

      - style.css: nueva clase .confirm-modal-380 (92vw por debajo de
        768px, 380px a partir de ahi -- el mismo comportamiento que
        w-[92vw] md:w-[380px], escrito a mano).
      - Reports.razor: el modal de "Generate Invoices" usa esa clase en
        vez de las utilidades de Tailwind que no estaban funcionando.

    Si con esto el modal ya sale a 380px, es señal de que hay algo en tu
    build de Tailwind que no esta recogiendo esa clase concreta (quiza
    valdria la pena mirarlo con calma en algun momento, pero ya no es
    urgente). Si te encuentras el mismo problema en otro sitio, dimelo y
    aplicamos el mismo truco ahi.

    Verificado: anchor de Reports.razor es el bloque COMPLETO que me
    pegaste tu mismo, tal cual, asi que deberia encajar exacto. Balance de
    llaves/parentesis limpio en los dos archivos.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-GenerateInvoicesModalWidth.ps1

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

# 1. Alakai.FestivalManager.Admin/wwwroot/assets/css/style.css -- style.css: clase .confirm-modal-380 con CSS normal (no clase de Tailwind) para forzar el ancho del modal de Generate Invoices
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/wwwroot/assets/css/style.css' `
    -Description 'style.css: clase .confirm-modal-380 con CSS normal (no clase de Tailwind) para forzar el ancho del modal de Generate Invoices' `
    -Anchor @'
.mud-button-label{
    display:contents;
}
'@ `
    -Replacement @'
.mud-button-label{
    display:contents;
}

/* Ancho fijo para el modal de confirmacion "Generate Invoices" en Reports,
   como CSS normal en vez de la clase de Tailwind w-[92vw] md:w-[380px] --
   esa clase, por lo que sea, no esta surtiendo efecto en tu build (se ha
   comprobado que el codigo del .razor ya la lleva bien puesta, y sigue
   saliendo el modal a pantalla completa incluso tras reiniciar el server).
   Esto no depende de que Tailwind compile nada nuevo: es CSS normal, en el
   mismo fichero que ya se sirve siempre. */
.confirm-modal-380 {
    width: 92vw;
}

@media (min-width: 768px) {
    .confirm-modal-380 {
        width: 380px;
    }
}
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: el modal de Generate Invoices usa la clase .confirm-modal-380 (CSS normal) en vez de la utilidad de Tailwind, que no esta surtiendo efecto
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: el modal de Generate Invoices usa la clase .confirm-modal-380 (CSS normal) en vez de la utilidad de Tailwind, que no esta surtiendo efecto' `
    -Anchor @'
@if (showBulkCreateInvoicesModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-[92vw] md:w-[380px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Generate Invoices</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/60">
                        This generates an invoice for every paid registration in this edition that doesn't have one yet, using the data already on file for each attendee (name, document, city, country -- no fiscal address, since the registration form never asks for one). This is for internal accounting, not fiscal invoices for attendees. It can only be done once per registration: it won't recreate invoices that already exist. Continue?
                    </p>
                </div>
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="CloseBulkCreateInvoicesModal">Cancel</button>
                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="ConfirmBulkCreateInvoicesAsync">@(downloadingReport == "invoice-pdfs-generate" ? "Generating..." : "Generate & Download")</button>
                </div>
            </div>
        </div>
    </div>
}
'@ `
    -Replacement @'
@if (showBulkCreateInvoicesModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative confirm-modal-380 overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Generate Invoices</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/60">
                        This generates an invoice for every paid registration in this edition that doesn't have one yet, using the data already on file for each attendee (name, document, city, country -- no fiscal address, since the registration form never asks for one). This is for internal accounting, not fiscal invoices for attendees. It can only be done once per registration: it won't recreate invoices that already exist. Continue?
                    </p>
                </div>
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="CloseBulkCreateInvoicesModal">Cancel</button>
                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="ConfirmBulkCreateInvoicesAsync">@(downloadingReport == "invoice-pdfs-generate" ? "Generating..." : "Generate & Download")</button>
                </div>
            </div>
        </div>
    </div>
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
Write-Host "Listo. Refresco fuerte del navegador (Ctrl+Shift+R) y prueba el modal" -ForegroundColor Cyan
Write-Host "de Generate Invoices en Reports." -ForegroundColor Cyan
Write-Host ""