# ============================================================================
# patch-accommodation-grid-report.ps1
# Replaces GenerateAccommodationGridReportAsync in ReportService.cs with the
# new "floor plan" layout (one sheet per building, zones as banners, unit
# blocks in a 4-per-row grid, all slots bordered, responsible in gray,
# booking-group number column).
#
# Run from the solution root (the folder containing
# Alakai.FestivalManager.Application). All-or-nothing: if any marker is not
# found exactly once, NOTHING is saved.
# ============================================================================

$ErrorActionPreference = 'Stop'

$path = 'Alakai.FestivalManager.Application\Features\Reports\Services\ReportService.cs'

if (-not (Test-Path $path)) {
    Write-Host "ERROR: File not found: $path" -ForegroundColor Red
    Write-Host "Run this script from the solution root." -ForegroundColor Red
    exit 1
}

# Read and normalize line endings before matching
$content = [System.IO.File]::ReadAllText($path)
$content = $content -replace "`r`n", "`n"

$startMarker = '    public async Task<byte[]> GenerateAccommodationGridReportAsync(Guid editionId, CancellationToken cancellationToken = default)'
$endMarker   = '    public async Task<byte[]> GenerateBusesReportAsync'

# --- Verification (all-or-nothing) ---
$startCount = ([regex]::Matches($content, [regex]::Escape($startMarker))).Count
$endCount   = ([regex]::Matches($content, [regex]::Escape($endMarker))).Count

if ($startCount -ne 1) {
    Write-Host "ABORTED: start marker found $startCount times (expected 1). Nothing was saved." -ForegroundColor Red
    exit 1
}
if ($endCount -ne 1) {
    Write-Host "ABORTED: end marker found $endCount times (expected 1). Nothing was saved." -ForegroundColor Red
    exit 1
}

$startIndex = $content.IndexOf($startMarker)
$endIndex   = $content.IndexOf($endMarker)

if ($startIndex -ge $endIndex) {
    Write-Host "ABORTED: markers are in unexpected order. Nothing was saved." -ForegroundColor Red
    exit 1
}

# --- New method (literal here-string: no PowerShell expansion happens here) ---
$newMethod = @'
    public async Task<byte[]> GenerateAccommodationGridReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccommodationBuilding> buildings = await _accommodationBuildingRepository.GetByEditionIdAsync(editionId, cancellationToken);
        IReadOnlyList<AccommodationReservation> reservations = await _accommodationReservationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        // Sequential booking number per edition (orders occupants of the same booking together,
        // same concept as the reservation number column in the legacy sheet).
        Dictionary<Guid, int> bookingNumbers = reservations
            .OrderBy(r => r.CreatedAt)
            .Select((r, index) => (r.Id, Number: index + 1))
            .ToDictionary(t => t.Id, t => t.Number);

        Dictionary<Guid, List<(AccommodationReservationOccupant Occupant, int BookingNumber)>> occupantsByUnit = reservations
            .SelectMany(r => r.Occupants.Select(o => (Occupant: o, BookingNumber: bookingNumbers[r.Id])))
            .Where(t => t.Occupant.AccommodationId.HasValue)
            .GroupBy(t => t.Occupant.AccommodationId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(t => t.BookingNumber)
                      .ThenByDescending(t => t.Occupant.IsResponsible)
                      .ThenBy(t => t.Occupant.Registration is not null ? $"{t.Occupant.Registration.FirstName} {t.Occupant.Registration.LastName}" : t.Occupant.Email, StringComparer.OrdinalIgnoreCase)
                      .ToList());

        // Layout constants: 4 unit blocks per band, each block = 2 columns (Name | Booking #),
        // with a 1-column spacer between blocks.
        const int BlocksPerBand = 4;
        const int ColumnsPerBlock = 2;
        const int SpacerColumns = 1;
        const int TotalColumns = BlocksPerBand * ColumnsPerBlock + (BlocksPerBand - 1) * SpacerColumns;

        XLColor buildingFill = XLColor.FromArgb(55, 65, 81);    // dark gray
        XLColor zoneFill = XLColor.FromArgb(156, 163, 175);     // medium gray
        XLColor unitHeaderFill = XLColor.FromArgb(107, 114, 128);
        XLColor responsibleFill = XLColor.FromArgb(209, 213, 219); // light gray
        XLColor borderColor = XLColor.FromArgb(107, 114, 128);

        using XLWorkbook workbook = new();
        HashSet<string> usedSheetNames = [];

        foreach (AccommodationBuilding building in buildings.OrderBy(b => b.SortOrder))
        {
            List<AccommodationZone> zonesWithUnits = building.Zones
                .OrderBy(z => z.SortOrder)
                .Where(z => z.Accommodations.Any(a => a.Capacity > 0))
                .ToList();

            if (zonesWithUnits.Count == 0)
            {
                continue;
            }

            string sheetName = MakeUniqueSheetName(building.Name, usedSheetNames);
            IXLWorksheet ws = workbook.Worksheets.Add(sheetName);
            ws.ShowGridLines = false;

            // Building title.
            IXLRange titleRange = ws.Range(1, 1, 1, TotalColumns).Merge();
            titleRange.Value = building.IsLocked ? $"{building.Name}  (Locked)" : building.Name;
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = buildingFill;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Row(1).Height = 24;

            // Legend.
            IXLRange legendRange = ws.Range(2, 1, 2, TotalColumns).Merge();
            legendRange.Value = "Gray row = booking responsible   ·   # = booking group";
            legendRange.Style.Font.Italic = true;
            legendRange.Style.Font.FontSize = 9;
            legendRange.Style.Font.FontColor = XLColor.FromArgb(107, 114, 128);

            int row = 4;

            foreach (AccommodationZone zone in zonesWithUnits)
            {
                // Zone header.
                IXLRange zoneRange = ws.Range(row, 1, row, TotalColumns).Merge();
                zoneRange.Value = zone.Name;
                zoneRange.Style.Font.Bold = true;
                zoneRange.Style.Fill.BackgroundColor = zoneFill;
                zoneRange.Style.Font.FontColor = XLColor.White;
                ws.Row(row).Height = 20;
                row += 2;

                List<Accommodation> zoneUnits = zone.Accommodations
                    .Where(a => a.Capacity > 0)
                    .OrderBy(a => NaturalSortKey(a.Name)).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                for (int chunkStart = 0; chunkStart < zoneUnits.Count; chunkStart += BlocksPerBand)
                {
                    List<Accommodation> band = zoneUnits.Skip(chunkStart).Take(BlocksPerBand).ToList();
                    int bandHeight = 0;

                    for (int blockIndex = 0; blockIndex < band.Count; blockIndex++)
                    {
                        Accommodation unit = band[blockIndex];
                        int startCol = 1 + blockIndex * (ColumnsPerBlock + SpacerColumns);

                        List<(AccommodationReservationOccupant Occupant, int BookingNumber)> occupants =
                            occupantsByUnit.TryGetValue(unit.Id, out List<(AccommodationReservationOccupant, int)>? list) ? list : [];

                        // Draw all capacity slots (occupied or empty); extend if overbooked so no one is lost.
                        int slotCount = Math.Max(unit.Capacity, occupants.Count);
                        bandHeight = Math.Max(bandHeight, slotCount);

                        // Unit header.
                        IXLRange unitHeader = ws.Range(row, startCol, row, startCol + ColumnsPerBlock - 1).Merge();
                        unitHeader.Value = $"{unit.Name}  ({unit.Capacity})";
                        unitHeader.Style.Font.Bold = true;
                        unitHeader.Style.Font.FontColor = XLColor.White;
                        unitHeader.Style.Fill.BackgroundColor = unitHeaderFill;
                        unitHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        unitHeader.Style.Border.OutsideBorderColor = borderColor;

                        for (int slot = 0; slot < slotCount; slot++)
                        {
                            IXLCell nameCell = ws.Cell(row + 1 + slot, startCol);
                            IXLCell bookingCell = ws.Cell(row + 1 + slot, startCol + 1);

                            if (slot < occupants.Count)
                            {
                                (AccommodationReservationOccupant occupant, int bookingNumber) = occupants[slot];
                                nameCell.Value = occupant.Registration is not null
                                    ? $"{occupant.Registration.FirstName} {occupant.Registration.LastName}"
                                    : occupant.Email;
                                bookingCell.Value = $"#{bookingNumber}";
                                bookingCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                bookingCell.Style.Font.FontSize = 9;
                                bookingCell.Style.Font.FontColor = XLColor.FromArgb(107, 114, 128);

                                if (occupant.IsResponsible)
                                {
                                    nameCell.Style.Font.Bold = true;
                                    nameCell.Style.Fill.BackgroundColor = responsibleFill;
                                    bookingCell.Style.Fill.BackgroundColor = responsibleFill;
                                }
                            }

                            IXLRange slotRange = ws.Range(row + 1 + slot, startCol, row + 1 + slot, startCol + 1);
                            slotRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            slotRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            slotRange.Style.Border.OutsideBorderColor = borderColor;
                            slotRange.Style.Border.InsideBorderColor = borderColor;
                        }
                    }

                    // Header row + slot rows + one blank spacer row after the band.
                    row += 1 + bandHeight + 1;
                }

                // Extra blank row between zones.
                row += 1;
            }

            // Fixed column widths (AdjustToContents would collapse the spacer columns).
            for (int blockIndex = 0; blockIndex < BlocksPerBand; blockIndex++)
            {
                int startCol = 1 + blockIndex * (ColumnsPerBlock + SpacerColumns);
                ws.Column(startCol).Width = 28;
                ws.Column(startCol + 1).Width = 7;

                if (blockIndex < BlocksPerBand - 1)
                {
                    ws.Column(startCol + 2).Width = 2;
                }
            }

            ws.SheetView.FreezeRows(2);
        }

        if (workbook.Worksheets.Count == 0)
        {
            workbook.Worksheets.Add("No data");
        }

        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
'@

# Normalize the replacement to LF as well (here-strings carry the script's own EOLs)
$newMethod = $newMethod -replace "`r`n", "`n"
if (-not $newMethod.EndsWith("`n")) { $newMethod += "`n" }
$newMethod += "`n"

# --- Splice ---
$patched = $content.Substring(0, $startIndex) + $newMethod + $content.Substring($endIndex)

# Restore CRLF and save (UTF-8 without BOM)
$patched = $patched -replace "`n", "`r`n"
[System.IO.File]::WriteAllText($path, $patched, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "OK: GenerateAccommodationGridReportAsync replaced successfully in $path" -ForegroundColor Green
Write-Host "Now rebuild: dotnet build" -ForegroundColor Cyan