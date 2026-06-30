# =====================================================================
# Alakai FestivalManager - Dashboard Phase 12
# Las sparklines de las 4 KPI cards ahora son VERDES si el % de cambio
# es positivo, ROJAS si es negativo, y GRISES si no hay dato de
# comparacion (periodo anterior con baseline 0). Ya no usan un color
# fijo arbitrario por tarjeta.
#
# Solo toca Dashboard.razor.
# USO: desde la raiz del repo -> .\dashboard-phase12.ps1
# =====================================================================

$ErrorActionPreference = "Stop"
Write-Host "Trabajando en: $(Get-Location)" -ForegroundColor Cyan

$dashboardRazorPath = "Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor"

if (-not (Test-Path $dashboardRazorPath)) {
    Write-Host "ERROR: no se encontro $dashboardRazorPath" -ForegroundColor Red
    exit 1
}

$content = Get-Content $dashboardRazorPath -Raw

# --- 1. Cambiar las 4 llamadas a BuildSparkline para pasar el % de cambio ---

$replacements = @(
    @{ Old = '@BuildSparkline(analytics.Overview.ViewsSparkline, "#7c3aed")';        New = '@BuildSparkline(analytics.Overview.ViewsSparkline, analytics.Overview.TotalViewsChangePercent)' }
    @{ Old = '@BuildSparkline(analytics.Overview.ActiveUsersSparkline, "#16a34a")';  New = '@BuildSparkline(analytics.Overview.ActiveUsersSparkline, analytics.Overview.ActiveUsersChangePercent)' }
    @{ Old = '@BuildSparkline(analytics.Overview.EventCountSparkline, "#d97706")';   New = '@BuildSparkline(analytics.Overview.EventCountSparkline, analytics.Overview.EventCountChangePercent)' }
    @{ Old = '@BuildSparkline(analytics.Overview.NewUsersSparkline, "#2563eb")';     New = '@BuildSparkline(analytics.Overview.NewUsersSparkline, analytics.Overview.NewUsersChangePercent)' }
)

$missing = @()
foreach ($r in $replacements) {
    if ($content.Contains($r.Old)) {
        $content = $content.Replace($r.Old, $r.New)
    } else {
        $missing += $r.Old
    }
}

if ($missing.Count -gt 0) {
    Write-Host "ERROR: no se encontraron estas llamadas exactas a BuildSparkline:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    Write-Host "No se modifico el archivo. Revisa manualmente o pide un full-rewrite." -ForegroundColor Yellow
    exit 1
}

Write-Host "  4 llamadas a BuildSparkline actualizadas." -ForegroundColor Green

# --- 2. Cambiar la firma/cuerpo de BuildSparkline para que reciba el % y elija color ---

$oldMethod = @'
    private static MarkupString BuildSparkline(List<long> values, string color)
    {
        if (values is null || values.Count < 2)
        {
            return new MarkupString(string.Empty);
        }

        long max = Math.Max(1, values.Max());
        double stepX = 300.0 / (values.Count - 1);

        StringBuilder points = new();

        for (int i = 0; i < values.Count; i++)
        {
            double x = i * stepX;
            double y = 46 - ((double)values[i] / max * 40);
            points.Append(x.ToString(CultureInfo.InvariantCulture));
            points.Append(',');
            points.Append(y.ToString(CultureInfo.InvariantCulture));
            points.Append(' ');
        }

        return new MarkupString($"<polyline points=\"{points.ToString().Trim()}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"2\" vector-effect=\"non-scaling-stroke\" />");
    }
'@

$newMethod = @'
    private static MarkupString BuildSparkline(List<long> values, decimal? changePercent)
    {
        if (values is null || values.Count < 2)
        {
            return new MarkupString(string.Empty);
        }

        // Color follows the same sign as the percent-change badge: green for growth,
        // red for decline, neutral gray when there is no previous-period baseline to compare against.
        string color = changePercent switch
        {
            > 0 => "#16a34a",
            < 0 => "#dc2626",
            _ => "#9ca3af"
        };

        long max = Math.Max(1, values.Max());
        double stepX = 300.0 / (values.Count - 1);

        StringBuilder points = new();

        for (int i = 0; i < values.Count; i++)
        {
            double x = i * stepX;
            double y = 46 - ((double)values[i] / max * 40);
            points.Append(x.ToString(CultureInfo.InvariantCulture));
            points.Append(',');
            points.Append(y.ToString(CultureInfo.InvariantCulture));
            points.Append(' ');
        }

        return new MarkupString($"<polyline points=\"{points.ToString().Trim()}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"2\" vector-effect=\"non-scaling-stroke\" />");
    }
'@

if ($content.Contains($oldMethod)) {
    $content = $content.Replace($oldMethod, $newMethod)
    Write-Host "  Metodo BuildSparkline actualizado (color por signo del %)." -ForegroundColor Green
} else {
    Write-Host "ERROR: no se encontro el cuerpo exacto de BuildSparkline. No se modifico el archivo." -ForegroundColor Red
    exit 1
}

Set-Content $dashboardRazorPath -Value $content -Encoding UTF8

Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "Listo. Sparklines: verde si sube, rojo si baja, gris si no hay" -ForegroundColor Cyan
Write-Host "periodo anterior con el que comparar. Compila y prueba." -ForegroundColor Yellow
Write-Host "=====================================================================" -ForegroundColor Cyan