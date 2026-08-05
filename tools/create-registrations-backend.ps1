#Requires -Version 7.0
<#
    Script 32 - Add "Check-in" downloadable Excel report to the Reports screen.

    Adds a new report type ("checkin") to the existing Reports architecture:
      - Alakai.FestivalManager.Application/Features/Reports/Services/IReportService.cs
          new interface method GenerateCheckInReportAsync
      - Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs
          new GenerateCheckInReportAsync method (Name, Pass Type, Level, Checked-in At)
          new BuildXlsx overload that highlights checked-in rows in light green
      - Alakai.FestivalManager.Api/Controllers/ReportsController.cs
          new "checkin" switch case + NotFound-check tuple entry
      - Alakai.FestivalManager.Admin/Components/Pages/Reports.razor
          new "Check-in" row with a Download button, between Registrations and Competitions

    Idempotent: safe to re-run. Uses the shared Patch-File helper (exact-string
    anchor match; SKIP on zero or multiple matches; never partial/silent writes).
#>

$ErrorActionPreference = 'Stop'

function Patch-File {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$OldString,
        [Parameter(Mandatory)][string]$NewString,
        [Parameter(Mandatory)][string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "  [SKIP] $Description -- file not found: $Path" -ForegroundColor Yellow
        return
    }

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")

    $content = $raw -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "  [SKIP] $Description -- already applied" -ForegroundColor Yellow
        return
    }

    $matches = [regex]::Matches($content, [regex]::Escape($oldNormalized))
    if ($matches.Count -eq 0) {
        Write-Host "  [SKIP] $Description -- anchor not found" -ForegroundColor Yellow
        return
    }
    if ($matches.Count -gt 1) {
        Write-Host "  [SKIP] $Description -- anchor ambiguous ($($matches.Count) matches)" -ForegroundColor Yellow
        return
    }

    $updated = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $updated = $updated -replace "`n", "`r`n"
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $updated, $utf8NoBom)
    Write-Host "  [OK]   $Description" -ForegroundColor Green
}

Write-Host "Script 32 - Add Check-in downloadable report" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# 1) IReportService.cs - new interface method
# ---------------------------------------------------------------------------
Patch-File `
    -Path ".\Alakai.FestivalManager.Application\Features\Reports\Services\IReportService.cs" `
    -Description "IReportService: add GenerateCheckInReportAsync" `
    -OldString @"
    Task<byte[]> GenerateRegistrationsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
"@ `
    -NewString @"
    Task<byte[]> GenerateRegistrationsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCheckInReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
"@

# ---------------------------------------------------------------------------
# 2a) ReportService.cs - new GenerateCheckInReportAsync method
# ---------------------------------------------------------------------------
Patch-File `
    -Path ".\Alakai.FestivalManager.Application\Features\Reports\Services\ReportService.cs" `
    -Description "ReportService: add GenerateCheckInReportAsync method" `
    -OldString @"
        return BuildXlsx("Registrations", ["First Name", "Last Name", "Email", "Pass Type", "Level", "Status", "Payment Status", "Final Price", "Discount Code", "Partner Email"], rows);
    }

    public async Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
"@ `
    -NewString @"
        return BuildXlsx("Registrations", ["First Name", "Last Name", "Email", "Pass Type", "Level", "Status", "Payment Status", "Final Price", "Discount Code", "Partner Email"], rows);
    }

    public async Task<byte[]> GenerateCheckInReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = registrations.Select(r => new[]
        {
            `$"{r.FirstName} {r.LastName}",
            r.PassType?.Name ?? "",
            r.Level?.Name ?? "",
            r.CheckedInAt.HasValue ? r.CheckedInAt.Value.ToString("yyyy-MM-dd HH:mm") : ""
        }).ToList();

        List<bool> highlightRows = registrations.Select(r => r.CheckedInAt.HasValue).ToList();

        return BuildXlsx("Check-in", ["Name", "Pass Type", "Level", "Checked-in At"], rows, highlightRows);
    }

    public async Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
"@

# ---------------------------------------------------------------------------
# 2b) ReportService.cs - new BuildXlsx overload with row highlighting
# ---------------------------------------------------------------------------
Patch-File `
    -Path ".\Alakai.FestivalManager.Application\Features\Reports\Services\ReportService.cs" `
    -Description "ReportService: add BuildXlsx overload with highlightRows" `
    -OldString @"
        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
"@ `
    -NewString @"
        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildXlsx(string sheetName, string[] headers, List<string[]> rows, List<bool> highlightRows)
    {
        using XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add(sheetName);

        for (int c = 0; c < headers.Length; c++)
        {
            IXLCell cell = worksheet.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(243, 244, 246);
        }

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < rows[r].Length; c++)
            {
                worksheet.Cell(r + 2, c + 1).Value = rows[r][c];
            }

            if (r < highlightRows.Count && highlightRows[r])
            {
                worksheet.Row(r + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
        }

        if (rows.Count > 0)
        {
            worksheet.RangeUsed()?.SetAutoFilter();
        }

        worksheet.Columns().AdjustToContents();

        using MemoryStream stream2 = new();
        workbook.SaveAs(stream2);
        return stream2.ToArray();
    }
}
"@

# ---------------------------------------------------------------------------
# 3) ReportsController.cs - switch case + NotFound-check tuple
# ---------------------------------------------------------------------------
Patch-File `
    -Path ".\Alakai.FestivalManager.Api\Controllers\ReportsController.cs" `
    -Description "ReportsController: add checkin switch case" `
    -OldString @"
            "registrations" => await _reportService.GenerateRegistrationsReportAsync(editionId, cancellationToken),
            "competitions" => await _reportService.GenerateCompetitionsReportAsync(editionId, cancellationToken),
"@ `
    -NewString @"
            "registrations" => await _reportService.GenerateRegistrationsReportAsync(editionId, cancellationToken),
            "checkin" => await _reportService.GenerateCheckInReportAsync(editionId, cancellationToken),
            "competitions" => await _reportService.GenerateCompetitionsReportAsync(editionId, cancellationToken),
"@

Patch-File `
    -Path ".\Alakai.FestivalManager.Api\Controllers\ReportsController.cs" `
    -Description "ReportsController: add checkin to NotFound-check tuple" `
    -OldString 'if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals" or "production-team" or "production-suppliers" or "production-trips" or "production-itineraries" or "production-accommodation" or "production-accommodation-grid"))' `
    -NewString 'if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "checkin" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals" or "production-team" or "production-suppliers" or "production-trips" or "production-itineraries" or "production-accommodation" or "production-accommodation-grid"))'

# ---------------------------------------------------------------------------
# 4) Reports.razor - new "Check-in" row
# ---------------------------------------------------------------------------
Patch-File `
    -Path ".\Alakai.FestivalManager.Admin\Components\Pages\Reports.razor" `
    -Description "Reports.razor: add Check-in row" `
    -OldString @"
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Registrations</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "registrations")" @onclick='() => DownloadAsync("registrations")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "registrations" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Competitions</td>
"@ `
    -NewString @"
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Registrations</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "registrations")" @onclick='() => DownloadAsync("registrations")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "registrations" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Check-in</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "checkin")" @onclick='() => DownloadAsync("checkin")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "checkin" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Competitions</td>
"@

Write-Host ""
Write-Host "Script 32 complete." -ForegroundColor Cyan