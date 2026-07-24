# Fix-ProductionReportsPersonalDataAndItinerarySections.ps1
#
# 1. Accommodation report: anade Email, Phone, Document Type, Document
#    Number y Nationality de cada ocupante.
# 2. Runner Itineraries report: deja de ser una tabla plana - ahora sale
#    seccionado por "Itinerary N" (mismo numero que la pantalla, por orden
#    cronologico), con un titulo por itinerario y sus viajes debajo.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function Patch-File {
    param([string]$Path, [string]$OldString, [string]$NewString, [string]$Description)
    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return $false }
    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"
    if ($normalizedContent.Contains($normalizedNew)) { Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan; return $true }
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$results = @()
$path = "Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs"

# ---------------------------------------------------------------------------
# 1) Accommodation: datos personales del ocupante
# ---------------------------------------------------------------------------
$results += Patch-File -Path $path -Description "Accommodation report: datos personales del ocupante" -OldString @'
        List<string[]> rows = reservations.SelectMany(r => r.Occupants.Select(o => new[]
        {
            r.ProductionAccommodationBuilding?.Name ?? "",
            r.ResponsibleProductionPerson is not null ? $"{r.ResponsibleProductionPerson.FirstName} {r.ResponsibleProductionPerson.LastName}" : "",
            o.ProductionPerson is not null ? $"{o.ProductionPerson.FirstName} {o.ProductionPerson.LastName}" : "",
            o.ProductionAccommodation?.ProductionAccommodationZone?.Name ?? "",
            o.ProductionAccommodation?.Name ?? ""
        })).ToList();

        return BuildXlsx("Production Accommodation", ["Building", "Responsible", "Occupant", "Zone", "Room"], rows);
'@ -NewString @'
        List<string[]> rows = reservations.SelectMany(r => r.Occupants.Select(o => new[]
        {
            r.ProductionAccommodationBuilding?.Name ?? "",
            r.ResponsibleProductionPerson is not null ? $"{r.ResponsibleProductionPerson.FirstName} {r.ResponsibleProductionPerson.LastName}" : "",
            o.ProductionPerson is not null ? $"{o.ProductionPerson.FirstName} {o.ProductionPerson.LastName}" : "",
            o.ProductionPerson?.Email ?? "",
            o.ProductionPerson?.Phone ?? "",
            o.ProductionPerson?.DocumentType.ToString() ?? "",
            o.ProductionPerson?.DocumentNumber ?? "",
            o.ProductionPerson?.Nationality ?? "",
            o.ProductionAccommodation?.ProductionAccommodationZone?.Name ?? "",
            o.ProductionAccommodation?.Name ?? ""
        })).ToList();

        return BuildXlsx("Production Accommodation", ["Building", "Responsible", "Occupant", "Email", "Phone", "Document Type", "Document Number", "Nationality", "Zone", "Room"], rows);
'@
if ($results -contains $false) { Write-Host "`nFallo (Accommodation report)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 2) Itineraries: informe seccionado por numero de itinerario
# ---------------------------------------------------------------------------
$results += Patch-File -Path $path -Description "Itineraries report: seccionado por Itinerary N" -OldString @'
    public async Task<byte[]> GenerateProductionItinerariesReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RunnerItinerary> itineraries = await _runnerItineraryRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = itineraries.SelectMany(i => i.Trips.Select(t => new[]
        {
            i.DateTime.ToString("dd/MM/yyyy HH:mm"), i.Location, i.Direction.ToString(), i.RunnerName ?? "",
            t.ProductionPerson is not null ? $"{t.ProductionPerson.FirstName} {t.ProductionPerson.LastName}" : "",
            t.TripNumber, t.DateTime.ToString("dd/MM/yyyy HH:mm"), t.TerminalOrStation
        })).ToList();

        return BuildXlsx("Itineraries", ["Itinerary Date/Time", "Location", "Direction", "Runner", "Person", "Trip Number", "Trip Time", "Terminal / Station"], rows);
    }
'@ -NewString @'
    public async Task<byte[]> GenerateProductionItinerariesReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RunnerItinerary> itineraries = await _runnerItineraryRepository.GetByEditionIdAsync(editionId, cancellationToken);
        List<RunnerItinerary> ordered = itineraries.OrderBy(i => i.DateTime).ToList();

        const int TotalColumns = 4;
        string[] tripHeaders = ["Person", "Trip Number", "Time", "Terminal / Station"];

        XLColor titleFill = XLColor.FromArgb(55, 65, 81);
        XLColor headerFill = XLColor.FromArgb(156, 163, 175);

        using XLWorkbook workbook = new();
        IXLWorksheet ws = workbook.Worksheets.Add("Itineraries");
        ws.ShowGridLines = false;

        int row = 1;

        for (int index = 0; index < ordered.Count; index++)
        {
            RunnerItinerary itinerary = ordered[index];
            int number = index + 1;

            string runnerSuffix = string.IsNullOrWhiteSpace(itinerary.RunnerName) ? "" : $"  ·  Runner: {itinerary.RunnerName}";

            IXLRange titleRange = ws.Range(row, 1, row, TotalColumns).Merge();
            titleRange.Value = $"Itinerary {number}  ·  {itinerary.Direction}  ·  {itinerary.Location}  ·  {itinerary.DateTime:dd/MM/yyyy HH:mm}{runnerSuffix}";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 13;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = titleFill;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Row(row).Height = 22;
            row++;

            for (int c = 0; c < tripHeaders.Length; c++)
            {
                IXLCell headerCell = ws.Cell(row, c + 1);
                headerCell.Value = tripHeaders[c];
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = headerFill;
                headerCell.Style.Font.FontColor = XLColor.White;
            }
            row++;

            List<ProductionTrip> tripsOrdered = itinerary.Trips.OrderBy(t => t.DateTime).ToList();

            if (tripsOrdered.Count == 0)
            {
                IXLCell emptyCell = ws.Cell(row, 1);
                emptyCell.Value = "No trips in this itinerary.";
                emptyCell.Style.Font.Italic = true;
                row++;
            }
            else
            {
                foreach (ProductionTrip trip in tripsOrdered)
                {
                    ws.Cell(row, 1).Value = trip.ProductionPerson is not null ? $"{trip.ProductionPerson.FirstName} {trip.ProductionPerson.LastName}" : "";
                    ws.Cell(row, 2).Value = trip.TripNumber;
                    ws.Cell(row, 3).Value = trip.DateTime.ToString("dd/MM/yyyy HH:mm");
                    ws.Cell(row, 4).Value = trip.TerminalOrStation;
                    row++;
                }
            }

            row++;
        }

        if (ordered.Count == 0)
        {
            ws.Cell(1, 1).Value = "No itineraries for this edition.";
        }

        for (int c = 1; c <= TotalColumns; c++)
        {
            ws.Column(c).Width = 26;
        }

        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
'@
if ($results -contains $false) { Write-Host "`nFallo (Itineraries report)." -ForegroundColor Red; exit 1 }

Write-Host "`nReports actualizados: Accommodation con datos personales, Itineraries seccionado por numero." -ForegroundColor Green