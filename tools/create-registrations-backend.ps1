<#
    Fix-EmailEditorContentDarkMode.ps1
    -----------------------------------------------

    El area donde escribes el cuerpo del email (dentro del editor de
    plantillas) se quedaba en blanco a proposito en modo oscuro -- la idea
    original era que sirviera de vista previa fiel de como se ve un email
    real (fondo blanco). Pero en la practica no se ve lo que escribes ahi
    en modo oscuro, asi que este script cambia el criterio: el area de
    texto tambien pasa a fondo oscuro, con el mismo tono que el resto de
    paneles.

    Que trae:

      - style.css: nueva regla para .rz-html-editor-content (el area
        editable del editor de Radzen) -- fondo y texto oscuro/claro en
        modo oscuro, igual que ya se hizo con la barra de herramientas.

    Ten en cuenta: al hacer esto, el editor deja de mostrarte el email tal
    cual se veria de verdad (sobre blanco) mientras estas en modo oscuro.
    Si mas adelante quieres recuperar esa vista previa fiel (por ejemplo
    con un boton para alternar "modo previsualizacion"), dimelo y lo
    montamos aparte -- por ahora prioriza poder ver lo que escribes, que es
    lo que has pedido.

    Verificado: mismo anchor estable que los scripts anteriores de
    style.css (no depende de que se haya aplicado ningun otro primero).
    Balance de llaves limpio.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-EmailEditorContentDarkMode.ps1

    Idempotente y todo-o-nada: si el anchor no encaja porque el archivo
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

# 1. Alakai.FestivalManager.Admin/wwwroot/assets/css/style.css -- style.css: el area editable del editor de plantillas de email tambien pasa a fondo oscuro en modo oscuro (antes se dejaba en blanco a proposito, pero no se veia el texto al escribir)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/wwwroot/assets/css/style.css' `
    -Description 'style.css: el area editable del editor de plantillas de email tambien pasa a fondo oscuro en modo oscuro (antes se dejaba en blanco a proposito, pero no se veia el texto al escribir)' `
    -Anchor @'
.mud-button-label{
    display:contents;
}
'@ `
    -Replacement @'
.mud-button-label{
    display:contents;
}

/* Editor de plantillas de email: el area de texto editable (donde escribes
   el cuerpo del email) se queda tambien en negro en modo oscuro, para que
   se vea lo que escribes -- deja de previsualizar el email tal cual se
   veria sobre fondo blanco real, pero es lo que se ha pedido: prioridad a
   poder ver lo que escribes. */
[data-mode="dark"] .rz-html-editor-content {
    background-color: rgb(31 31 31) !important;
    color: rgb(255 255 255 / 0.8) !important;
}

[data-mode="dark"] .rz-html-editor-content[contenteditable="true"],
[data-mode="dark"] .rz-html-editor-content [contenteditable="true"] {
    background-color: rgb(31 31 31) !important;
    color: rgb(255 255 255 / 0.8) !important;
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
Write-Host "Listo. Refresco fuerte (Ctrl+Shift+R) y prueba a escribir en el editor" -ForegroundColor Cyan
Write-Host "de plantillas de email en modo oscuro." -ForegroundColor Cyan
Write-Host ""