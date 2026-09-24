<#
    Fix-DashboardRevenueDailyOption.ps1
    -----------------------------------------------

    IMPORTANTE -- este script depende de TODOS los anteriores del dashboard,
    incluido Fix-DashboardFinanceGrid3x2.ps1 (el ultimo que te mande). Aplica
    todos esos ANTES que este, en ese orden.

    Responde a tu pregunta sobre una franja diaria en el grafico de Revenue:

      - Nueva opcion "Daily" en el selector de rango (antes de "Weekly"):
        muestra los ultimos 15 dias, uno por dia. Reutiliza BuildDailyPoints,
        que ya existia en el repositorio (se usaba para el rango libre de
        fechas) pero no estaba conectado a ningun valor del selector.
      - Las flechas de navegacion (que ya funcionaban en Weekly/Monthly)
        tambien funcionan ahora en Daily: cada clic mueve la ventana 15 dias
        hacia atras o hacia delante, igual que semanas/meses en las otras
        vistas.

    Verificado con un navegador real (Playwright + el bundle exacto de
    ApexCharts): 15 puntos diarios se ven bien, sin amontonarse, con el
    mismo formato en euros y franjas del eje Y que ya tenia el grafico.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-DashboardRevenueDailyOption.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado (por ejemplo porque falta aplicar alguno de
    los scripts anteriores), no escribe nada y lista el problema.
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
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- revisalo a mano, o falta aplicar alguno de los scripts anteriores). Descripcion: $($Op.Description)"
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

# 1. Alakai.FestivalManager.Application/Interfaces/Repositories/IDashboardRepository.cs -- Doc comment: nueva opcion de rango 'day'
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Interfaces/Repositories/IDashboardRepository.cs' `
    -Description 'Doc comment: nueva opcion de rango ''day''' `
    -Anchor @'
    /// <param name="range">"week" (last 8 calendar weeks, one point per week), "month" (last 6 calendar months, one point per month), "quarter" (last 4 natural calendar quarters, quarterly) or "year" (last 12 months, monthly). Ignored when <paramref name="customStart"/> and <paramref name="customEnd"/> are both provided.</param>
    /// <param name="offset">How many periods back from the most recent to anchor "week"/"month" on (0 = current period). Ignored for "quarter", "year" and the custom range.</param>
'@ `
    -Replacement @'
    /// <param name="range">"day" (last 15 calendar days, one point per day), "week" (last 8 calendar weeks, one point per week), "month" (last 6 calendar months, one point per month), "quarter" (last 4 natural calendar quarters, quarterly) or "year" (last 12 months, monthly). Ignored when <paramref name="customStart"/> and <paramref name="customEnd"/> are both provided.</param>
    /// <param name="offset">How many periods back from the most recent to anchor "day"/"week"/"month" on (0 = current period). Ignored for "quarter", "year" and the custom range.</param>
'@

# 2. Alakai.FestivalManager.Infrastructure/Repositories/DashboardRepository.cs -- Nueva rama 'day' en GetRevenueAsync: ultimos 15 dias, navegable con offset igual que week/month
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Repositories/DashboardRepository.cs' `
    -Description 'Nueva rama ''day'' en GetRevenueAsync: ultimos 15 dias, navegable con offset igual que week/month' `
    -Anchor @'
        int normalizedOffset = Math.Max(0, offset);

        if (string.Equals(range, "week", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWeeklyPoints(paidRegistrations, Effective, 8, normalizedOffset);
        }
'@ `
    -Replacement @'
        int normalizedOffset = Math.Max(0, offset);

        if (string.Equals(range, "day", StringComparison.OrdinalIgnoreCase))
        {
            DateOnly anchorEnd = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-15 * normalizedOffset);
            DateOnly anchorStart = anchorEnd.AddDays(-14);
            return BuildDailyPoints(paidRegistrations, Effective, anchorStart, anchorEnd, "dd MMM");
        }

        if (string.Equals(range, "week", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWeeklyPoints(paidRegistrations, Effective, 8, normalizedOffset);
        }
'@

# 3. Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor -- Flechas de navegacion tambien visibles en la vista Diaria
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor' `
    -Description 'Flechas de navegacion tambien visibles en la vista Diaria' `
    -Anchor @'
                        @if (revenueRange is "week" or "month")
                        {
'@ `
    -Replacement @'
                        @if (revenueRange is "day" or "week" or "month")
                        {
'@

# 4. Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor -- Nueva opcion 'Daily' (ultimos 15 dias) en el selector de rango de Revenue
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor' `
    -Description 'Nueva opcion ''Daily'' (ultimos 15 dias) en el selector de rango de Revenue' `
    -Anchor @'
                        <select class="form-select" style="width:auto; max-width:160px;" @bind="revenueRange" @bind:after="OnRevenueRangeChangedAsync">
                            <option value="week">Weekly</option>
'@ `
    -Replacement @'
                        <select class="form-select" style="width:auto; max-width:160px;" @bind="revenueRange" @bind:after="OnRevenueRangeChangedAsync">
                            <option value="day">Daily</option>
                            <option value="week">Weekly</option>
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
    Write-Host "Lo mas probable es que alguno de los scripts anteriores del dashboard no" -ForegroundColor Yellow
    Write-Host "se haya aplicado todavia, o que algun archivo se haya editado desde entonces." -ForegroundColor Yellow
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
Write-Host "Listo. Siguientes pasos:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1) Revisa el diff (git diff) antes de compilar." -ForegroundColor White
Write-Host "  2) dotnet build (no hace falta migracion, este script no toca la BD)." -ForegroundColor White
Write-Host "  3) Prueba la opcion Daily del grafico de Revenue y sus flechas." -ForegroundColor White
Write-Host ""