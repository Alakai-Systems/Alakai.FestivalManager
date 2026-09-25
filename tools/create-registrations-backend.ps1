<#
    Fix-DashboardFinanceReportsRefundedAmount.ps1
    -----------------------------------------------

    Correccion sobre Fix-DashboardFinanceReports.ps1: los 3 reports
    financieros que muestran un importe "Pending" (pendiente de cobro)
    calculaban FinalPrice - AmountPaid sin contar los reembolsos.

    Como AmountPaid nunca baja al reembolsar (se guarda aparte en
    RefundedAmount, a proposito, para no perder el historico de lo
    cobrado -- lo introdujo Fix-RedsysRefunds.ps1), en cuanto una
    inscripcion tenia algun reembolso su "pendiente" en estos reports
    se quedaba en 0 aunque en realidad siguiera habiendo saldo por
    cobrar. Es el mismo bug, y la misma correccion, que el que arregle
    en Fix-RedsysRepayAndBasePrice.ps1 para el Panel de Usuario:

        pendiente = FinalPrice - AmountPaid + RefundedAmount

    REQUISITO: este script da por hecho que ya tienes aplicados, en este
    orden, Fix-FestivalCurrency.ps1, Fix-RedsysRefunds.ps1 y
    Fix-DashboardFinanceReports.ps1 (los tres reports que toca este
    script los creo ese ultimo).

    Que cambia, archivo unico (ReportService.cs):

      1) Financial Summary -- "Pending collection" (el numero global) y
         la columna "Pending" del desglose por tipo de pase.
      2) Payments Detail -- columna "Pending" del listado fila a fila.
      3) Outstanding Balances -- la formula de la columna "Pending Amount"
         Y el filtro que decide quien aparece como deudor: antes se
         excluia sin mas a cualquier inscripcion en estado Refunded; con
         la decision que tomaste de dejar el repago siempre disponible,
         una inscripcion Refunded puede seguir debiendo dinero de verdad
         (por ejemplo si solo se habia pagado una parte y esa parte se
         reembolso), asi que ahora tambien puede salir en este listado si
         el calculo dice que debe algo.

    No se toca "Refunded (cancelled/refunded regs.)" ni "Net revenue" del
    Financial Summary -- esas dos filas no tenian el mismo problema (no
    usan FinalPrice - AmountPaid), asi que se quedan como estaban.

    Verificado contra los archivos reales del repo (anchors unicos, balance
    de llaves/parentesis/corchetes limpio, diff revisado).

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-DashboardFinanceReportsRefundedAmount.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado (por ejemplo, porque los prerrequisitos no
    estan aplicados), no escribe nada y lista el problema.
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
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- revisa si tienes aplicados Fix-FestivalCurrency.ps1, Fix-RedsysRefunds.ps1 y Fix-DashboardFinanceReports.ps1, o si el archivo difiere por otro motivo). Descripcion: $($Op.Description)"
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

# 1. Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs -- Financial Summary: 'Pending collection' cuenta RefundedAmount (si no, da 0 tras un reembolso)
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs' `
    -Description 'Financial Summary: ''Pending collection'' cuenta RefundedAmount (si no, da 0 tras un reembolso)' `
    -Anchor @'
    public async Task<byte[]> GenerateFinancialSummaryReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);
        List<Registration> active = registrations.Where(r => r.Status != RegistrationStatus.Cancelled).ToList();
        List<Registration> refundedOrCancelled = registrations.Where(r => r.Status == RegistrationStatus.Cancelled || r.PaymentStatus == PaymentStatus.Refunded).ToList();

        decimal grossRevenue = active.Sum(r => r.FinalPrice);
        decimal collected = active.Sum(r => r.AmountPaid);
        decimal pending = Math.Max(0m, grossRevenue - collected);
        decimal refunded = refundedOrCancelled.Sum(r => r.AmountPaid);
'@ `
    -Replacement @'
    public async Task<byte[]> GenerateFinancialSummaryReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);
        List<Registration> active = registrations.Where(r => r.Status != RegistrationStatus.Cancelled).ToList();
        List<Registration> refundedOrCancelled = registrations.Where(r => r.Status == RegistrationStatus.Cancelled || r.PaymentStatus == PaymentStatus.Refunded).ToList();

        decimal grossRevenue = active.Sum(r => r.FinalPrice);
        decimal collected = active.Sum(r => r.AmountPaid);
        // AmountPaid nunca baja al reembolsar (se guarda aparte en RefundedAmount),
        // asi que hay que sumarselo de vuelta para saber lo que de verdad queda
        // pendiente; si no, en cuanto hay un reembolso esto da 0 aunque siga
        // habiendo saldo real por cobrar.
        decimal totalRefunded = active.Sum(r => r.RefundedAmount);
        decimal pending = Math.Max(0m, grossRevenue - collected + totalRefunded);
        decimal refunded = refundedOrCancelled.Sum(r => r.AmountPaid);
'@

# 2. Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs -- Financial Summary: columna Pending del desglose por tipo de pase cuenta RefundedAmount
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs' `
    -Description 'Financial Summary: columna Pending del desglose por tipo de pase cuenta RefundedAmount' `
    -Anchor @'
        foreach (IGrouping<string, Registration> group in byPassType)
        {
            decimal groupGross = group.Sum(r => r.FinalPrice);
            decimal groupCollected = group.Sum(r => r.AmountPaid);

            ws.Cell(row, 1).Value = group.Key;
            ws.Cell(row, 2).Value = group.Count();
            ws.Cell(row, 3).Value = groupGross.ToString("0.00");
            ws.Cell(row, 4).Value = groupCollected.ToString("0.00");
            ws.Cell(row, 5).Value = Math.Max(0m, groupGross - groupCollected).ToString("0.00");
            row++;
        }
'@ `
    -Replacement @'
        foreach (IGrouping<string, Registration> group in byPassType)
        {
            decimal groupGross = group.Sum(r => r.FinalPrice);
            decimal groupCollected = group.Sum(r => r.AmountPaid);
            decimal groupRefunded = group.Sum(r => r.RefundedAmount);

            ws.Cell(row, 1).Value = group.Key;
            ws.Cell(row, 2).Value = group.Count();
            ws.Cell(row, 3).Value = groupGross.ToString("0.00");
            ws.Cell(row, 4).Value = groupCollected.ToString("0.00");
            ws.Cell(row, 5).Value = Math.Max(0m, groupGross - groupCollected + groupRefunded).ToString("0.00");
            row++;
        }
'@

# 3. Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs -- Payments Detail: columna Pending cuenta RefundedAmount
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs' `
    -Description 'Payments Detail: columna Pending cuenta RefundedAmount' `
    -Anchor @'
            r.FinalPrice.ToString("0.00"), r.AmountPaid.ToString("0.00"), (r.FinalPrice - r.AmountPaid).ToString("0.00"),
'@ `
    -Replacement @'
            r.FinalPrice.ToString("0.00"), r.AmountPaid.ToString("0.00"), (r.FinalPrice - r.AmountPaid + r.RefundedAmount).ToString("0.00"),
'@

# 4. Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs -- Outstanding Balances: filtro y columna Pending cuentan RefundedAmount (y ya no se excluye a quien esta en estado Refunded si de verdad debe dinero)
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs' `
    -Description 'Outstanding Balances: filtro y columna Pending cuentan RefundedAmount (y ya no se excluye a quien esta en estado Refunded si de verdad debe dinero)' `
    -Anchor @'
        List<Registration> debtors = registrations
            .Where(r => r.Status != RegistrationStatus.Cancelled && r.PaymentStatus != PaymentStatus.Refunded && (r.FinalPrice - r.AmountPaid) > 0)
            .OrderBy(r => r.PaymentDueAt ?? DateTime.MaxValue)
            .ToList();

        DateTime today = DateTime.UtcNow.Date;

        List<string[]> rows = debtors.Select(r => new[]
        {
            r.FirstName, r.LastName, r.Email, r.Phone ?? "",
            r.PassType?.Name ?? "", r.PaymentPlan.ToString(),
            (r.FinalPrice - r.AmountPaid).ToString("0.00"),
            r.PaymentDueAt.HasValue ? r.PaymentDueAt.Value.ToString("dd/MM/yyyy") : "",
            r.PaymentDueAt.HasValue && r.PaymentDueAt.Value.Date < today ? (today - r.PaymentDueAt.Value.Date).Days.ToString() : "",
            r.PaymentStatus.ToString()
        }).ToList();
'@ `
    -Replacement @'
        List<Registration> debtors = registrations
            .Where(r => r.Status != RegistrationStatus.Cancelled && (r.FinalPrice - r.AmountPaid + r.RefundedAmount) > 0)
            .OrderBy(r => r.PaymentDueAt ?? DateTime.MaxValue)
            .ToList();

        DateTime today = DateTime.UtcNow.Date;

        List<string[]> rows = debtors.Select(r => new[]
        {
            r.FirstName, r.LastName, r.Email, r.Phone ?? "",
            r.PassType?.Name ?? "", r.PaymentPlan.ToString(),
            (r.FinalPrice - r.AmountPaid + r.RefundedAmount).ToString("0.00"),
            r.PaymentDueAt.HasValue ? r.PaymentDueAt.Value.ToString("dd/MM/yyyy") : "",
            r.PaymentDueAt.HasValue && r.PaymentDueAt.Value.Date < today ? (today - r.PaymentDueAt.Value.Date).Days.ToString() : "",
            r.PaymentStatus.ToString()
        }).ToList();
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
    Write-Host "Revisa esos archivos a mano, o dime que ha cambiado para regenerar el script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Listo. Siguientes pasos:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1) Revisa el diff (git diff) antes de compilar." -ForegroundColor White
Write-Host "  2) dotnet build (no hace falta migracion, este script no toca la BD)." -ForegroundColor White
Write-Host "  3) Descarga Financial Summary, Payments Detail y Outstanding Balances desde" -ForegroundColor White
Write-Host "     /reports (pestana Finance) con datos que incluyan algun reembolso, y" -ForegroundColor White
Write-Host "     comprueba que el pendiente ya no sale en 0." -ForegroundColor White
Write-Host ""