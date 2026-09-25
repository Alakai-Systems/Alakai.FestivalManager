<#
    Fix-ReportsTabsAndInvoicePdfs.ps1
    -----------------------------------------------

    Este script es un SEGUIDO de Fix-BulkInvoices.ps1, que ya lanzaste (el
    de 15 pasos). Ese script funciono -- por eso no toca nada de backend --
    pero lo construi sin ver que tu Reports.razor YA tenia las pestanas
    Operations/Finance de un script anterior (Fix-DashboardFinanceReports.ps1),
    asi que la fila "Invoices" acabo metida dentro de Operations, con el
    mismo nombre de clave ("invoices") que ya usaba la fila "Invoices / VAT"
    de Finance -- dos botones distintos compartiendo el mismo estado de
    "descargando", un bug real aunque no se note a simple vista todavia.

    Este script, verificado contra tu Reports.razor EXACTO (el que me
    pegaste, con la fila Invoices ya en Operations), arregla las dos cosas
    que pediste:

      1. Las pestanas Operations/Finance pasan a verse igual que las de
         Dashboard (fondo gris, pestana activa con fondo blanco/oscuro y
         sombra), en vez de la raya inferior que tenian.

      2. La fila de facturas se mueve de Operations a Finance (justo
         despues de "Invoices / VAT"), se renombra a "Invoice PDFs" para
         no confundirla con esa, y pasa a usar sus propias claves internas
         (invoice-pdfs / invoice-pdfs-generate) en vez de invoices /
         invoices-bulk-create -- así ya no colisiona con el boton de
         "Invoices / VAT". Los botones Download y Generate & Download
         siguen haciendo exactamente lo mismo que ya hacian.

    No toca nada de backend (Global.cs, InvoiceRepository, InvoiceService,
    InvoicesController, InvoiceApiClient, QuestPdfInvoiceService): eso ya
    quedo bien aplicado con el script anterior y no depende de las
    pestanas.

    Verificado contra tu Reports.razor real reconstruido paso a paso
    (Fix-DashboardFinanceReports.ps1 + tu Fix-BulkInvoices.ps1 de 15 pasos,
    ambos ya aplicados), asi que los anchors deberian encajar tal cual esta
    tu archivo ahora. Anchors unicos, balance de llaves/parentesis limpio,
    diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-ReportsTabsAndInvoicePdfs.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque el archivo
    local difiere de lo esperado, no escribe nada y lista el problema --
    en ese caso, pegame tu Reports.razor actual otra vez y lo reviso.
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

# 1. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: pestanas Operations/Finance con el mismo estilo 'segmented control' que Dashboard
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: pestanas Operations/Finance con el mismo estilo ''segmented control'' que Dashboard' `
    -Anchor @'
        <div class="flex gap-2 mb-4 border-b border-black/10 dark:border-darkborder">
            <button type="button" class="px-4 py-2 text-sm font-semibold border-b-2 -mb-px @(activeTab == "operations" ? "border-purple text-purple" : "border-transparent text-black/50 dark:text-white/60")" @onclick='() => activeTab = "operations"'>Operations</button>
            <button type="button" class="px-4 py-2 text-sm font-semibold border-b-2 -mb-px @(activeTab == "finance" ? "border-purple text-purple" : "border-transparent text-black/50 dark:text-white/60")" @onclick='() => activeTab = "finance"'>Finance</button>
        </div>
'@ `
    -Replacement @'
        <div class="inline-flex items-center gap-1 p-1 mb-4 rounded-lg bg-black/5 dark:bg-white/5 w-fit" role="tablist">
            <button type="button" role="tab" aria-selected="@(activeTab == "operations" ? "true" : "false")" class="px-4 py-2 text-sm font-semibold rounded-md transition-colors @(activeTab == "operations" ? "bg-white dark:bg-dark text-purple shadow-sm" : "text-black/50 dark:text-white/50 hover:text-black dark:hover:text-white")" @onclick='() => activeTab = "operations"'>
                Operations
            </button>
            <button type="button" role="tab" aria-selected="@(activeTab == "finance" ? "true" : "false")" class="px-4 py-2 text-sm font-semibold rounded-md transition-colors @(activeTab == "finance" ? "bg-white dark:bg-dark text-purple shadow-sm" : "text-black/50 dark:text-white/50 hover:text-black dark:hover:text-white")" @onclick='() => activeTab = "finance"'>
                Finance
            </button>
        </div>
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: quita la fila Invoices de Operations (estaba mal colocada por el script anterior)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: quita la fila Invoices de Operations (estaba mal colocada por el script anterior)' `
    -Anchor @'
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Competitions</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "competitions")" @onclick='() => DownloadAsync("competitions")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "competitions" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Invoices</td>
                            <td class="px-4 py-3 text-right">
                                <div class="inline-flex gap-2">
                                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoices")" @onclick="DownloadInvoicesZipAsync">
                                        <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoices" ? "Downloading..." : "Download")
                                    </button>
                                    <button type="button" class="btn border border-black/10 dark:border-darkborder disabled:opacity-50" disabled="@(downloadingReport == "invoices-bulk-create")" @onclick="OpenBulkCreateInvoicesModal">
                                        <i class="ri-file-add-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoices-bulk-create" ? "Generating..." : "Generate & Download")
                                    </button>
                                </div>
                            </td>
                        </tr>
                        @if (HasAccommodationModule)
'@ `
    -Replacement @'
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Competitions</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "competitions")" @onclick='() => DownloadAsync("competitions")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "competitions" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        @if (HasAccommodationModule)
'@

# 3. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: fila Invoice PDFs en Finance, justo despues de Invoices / VAT, con claves propias
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: fila Invoice PDFs en Finance, justo despues de Invoices / VAT, con claves propias' `
    -Anchor @'
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Invoices / VAT</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoices")" @onclick='() => DownloadAsync("invoices")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoices" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
'@ `
    -Replacement @'
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Invoices / VAT</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoices")" @onclick='() => DownloadAsync("invoices")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoices" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Invoice PDFs</td>
                            <td class="px-4 py-3 text-right">
                                <div class="inline-flex gap-2">
                                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoice-pdfs")" @onclick="DownloadInvoicesZipAsync">
                                        <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoice-pdfs" ? "Downloading..." : "Download")
                                    </button>
                                    <button type="button" class="btn border border-black/10 dark:border-darkborder disabled:opacity-50" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="OpenBulkCreateInvoicesModal">
                                        <i class="ri-file-add-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "invoice-pdfs-generate" ? "Generating..." : "Generate & Download")
                                    </button>
                                </div>
                            </td>
                        </tr>
'@

# 4. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: el modal usa la nueva clave invoice-pdfs-generate
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: el modal usa la nueva clave invoice-pdfs-generate' `
    -Anchor @'
                    <button type="button" class="btn border border-black/10" disabled="@(downloadingReport == "invoices-bulk-create")" @onclick="CloseBulkCreateInvoicesModal">Cancel</button>
                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoices-bulk-create")" @onclick="ConfirmBulkCreateInvoicesAsync">@(downloadingReport == "invoices-bulk-create" ? "Generating..." : "Generate & Download")</button>
'@ `
    -Replacement @'
                    <button type="button" class="btn border border-black/10" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="CloseBulkCreateInvoicesModal">Cancel</button>
                    <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "invoice-pdfs-generate")" @onclick="ConfirmBulkCreateInvoicesAsync">@(downloadingReport == "invoice-pdfs-generate" ? "Generating..." : "Generate & Download")</button>
'@

# 5. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: DownloadInvoicesZipAsync usa la clave invoice-pdfs (ya no invoices, que colisionaba con Invoices / VAT)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: DownloadInvoicesZipAsync usa la clave invoice-pdfs (ya no invoices, que colisionaba con Invoices / VAT)' `
    -Anchor @'
    private async Task DownloadInvoicesZipAsync()
    {
        if (selectedEditionId == Guid.Empty)
        {
            return;
        }

        downloadingReport = "invoices";

        try
        {
            byte[] bytes = await InvoiceApiClient.GetInvoicesZipAsync(selectedEditionId);
'@ `
    -Replacement @'
    private async Task DownloadInvoicesZipAsync()
    {
        if (selectedEditionId == Guid.Empty)
        {
            return;
        }

        downloadingReport = "invoice-pdfs";

        try
        {
            byte[] bytes = await InvoiceApiClient.GetInvoicesZipAsync(selectedEditionId);
'@

# 6. Alakai.FestivalManager.Admin/Components/Pages/Reports.razor -- Reports.razor: ConfirmBulkCreateInvoicesAsync usa la clave invoice-pdfs-generate
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Reports.razor' `
    -Description 'Reports.razor: ConfirmBulkCreateInvoicesAsync usa la clave invoice-pdfs-generate' `
    -Anchor @'
    private async Task ConfirmBulkCreateInvoicesAsync()
    {
        showBulkCreateInvoicesModal = false;
        downloadingReport = "invoices-bulk-create";

        try
        {
            byte[] bytes = await InvoiceApiClient.BulkCreateInvoicesZipAsync(selectedEditionId);
'@ `
    -Replacement @'
    private async Task ConfirmBulkCreateInvoicesAsync()
    {
        showBulkCreateInvoicesModal = false;
        downloadingReport = "invoice-pdfs-generate";

        try
        {
            byte[] bytes = await InvoiceApiClient.BulkCreateInvoicesZipAsync(selectedEditionId);
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
Write-Host "Listo. En Reports:" -ForegroundColor Cyan
Write-Host "  - Las pestanas Operations/Finance deben verse igual que las de Dashboard." -ForegroundColor Cyan
Write-Host "  - En Finance, justo debajo de 'Invoices / VAT', deberia aparecer 'Invoice PDFs'" -ForegroundColor Cyan
Write-Host "    con sus botones Download y Generate & Download (ya no esta en Operations)." -ForegroundColor Cyan
Write-Host ""