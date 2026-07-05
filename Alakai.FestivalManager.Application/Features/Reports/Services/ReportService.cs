using ClosedXML.Excel;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Reports.Services;

public class ReportService : IReportService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IAccommodationReservationRepository _accommodationReservationRepository;
    private readonly IAccommodationBuildingRepository _accommodationBuildingRepository;
    private readonly IBusReservationRepository _busReservationRepository;
    private readonly IMealPreferenceRepository _mealPreferenceRepository;

    public ReportService(
        IRegistrationRepository registrationRepository,
        ICompetitionEntryRepository competitionEntryRepository,
        IAccommodationReservationRepository accommodationReservationRepository,
        IAccommodationBuildingRepository accommodationBuildingRepository,
        IBusReservationRepository busReservationRepository,
        IMealPreferenceRepository mealPreferenceRepository)
    {
        _registrationRepository = registrationRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _accommodationBuildingRepository = accommodationBuildingRepository;
        _busReservationRepository = busReservationRepository;
        _mealPreferenceRepository = mealPreferenceRepository;
    }

    public async Task<byte[]> GenerateUsersReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = registrations.Select(r => new[]
        {
            r.FirstName, r.LastName, r.Email, r.Phone ?? "", r.Country ?? "", r.City ?? ""
        }).ToList();

        return BuildXlsx("Users", ["First Name", "Last Name", "Email", "Phone", "Country", "City"], rows);
    }

    public async Task<byte[]> GenerateRegistrationsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = registrations.Select(r => new[]
        {
            r.FirstName, r.LastName, r.Email,
            r.PassType?.Name ?? "", r.Level?.Name ?? "",
            r.Status.ToString(), r.PaymentStatus.ToString(),
            r.FinalPrice.ToString("0.00"), r.DiscountCodeValue ?? "", r.PartnerEmail ?? ""
        }).ToList();

        return BuildXlsx("Registrations", ["First Name", "Last Name", "Email", "Pass Type", "Level", "Status", "Payment Status", "Final Price", "Discount Code", "Partner Email"], rows);
    }

    public async Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = entries.Select(e => new[]
        {
            e.Registration is not null ? $"{e.Registration.FirstName} {e.Registration.LastName}" : "",
            e.Registration?.Email ?? "",
            e.Competition?.Name ?? "",
            e.CompetitionCapacity?.CompetitionLevel?.Name ?? "Open",
            e.DanceRole?.ToString() ?? (e.TeamName ?? "Individual"),
            e.Status.ToString()
        }).ToList();

        return BuildXlsx("Competitions", ["Name", "Email", "Competition", "Level", "Role / Team", "Status"], rows);
    }

    public async Task<byte[]> GenerateAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccommodationReservation> reservations = await _accommodationReservationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = reservations.SelectMany(r => r.Occupants.Select(o => new[]
        {
            o.Registration is not null ? $"{o.Registration.FirstName} {o.Registration.LastName}" : o.Email,
            o.Registration?.Email ?? o.Email,
            r.AccommodationBuilding?.Name ?? "",
            o.Accommodation?.AccommodationZone?.Name ?? "",
            o.Accommodation?.Name ?? "",
            o.IsResponsible ? "Yes" : "No"
        })).ToList();

        return BuildXlsx("Accommodation", ["Name", "Email", "Building", "Zone", "Unit", "Responsible"], rows);
    }

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

    public async Task<byte[]> GenerateBusesReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BusReservation> reservations = await _busReservationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = reservations.Select(r => new[]
        {
            r.Registration is not null ? $"{r.Registration.FirstName} {r.Registration.LastName}" : "",
            r.Registration?.Email ?? "",
            r.Bus?.Direction.ToString() ?? "",
            r.Bus?.DepartureTime.ToString("dd/MM/yyyy HH:mm") ?? "",
            r.Bus?.PickupLocation ?? "",
            r.Bus?.DestinationLocation ?? "",
            r.Bus?.Price.ToString("0.00") ?? ""
        }).ToList();

        return BuildXlsx("Buses", ["Name", "Email", "Direction", "Departure", "Pickup", "Destination", "Price"], rows);
    }

    public async Task<byte[]> GenerateMealsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);
        IReadOnlyList<MealPreference> preferences = await _mealPreferenceRepository.GetByEditionIdAsync(editionId, cancellationToken);

        Dictionary<Guid, MealPreference> byRegistration = preferences.ToDictionary(p => p.RegistrationId, p => p);

        List<string[]> rows = registrations.Select(r =>
        {
            byRegistration.TryGetValue(r.Id, out MealPreference? preference);

            return new[]
            {
                r.FirstName, r.LastName, r.Email,
                preference is null ? "Not set" : preference.MenuType.ToString(),
                preference is not null && preference.IsCeliacOrGlutenIntolerant ? "Yes" : "No",
                preference?.AllergiesNotes ?? ""
            };
        }).ToList();

        return BuildXlsx("Meals", ["First Name", "Last Name", "Email", "Menu", "Celiac / Gluten", "Allergies"], rows);
    }

    private static int NaturalSortKey(string name)
    {
        return int.TryParse(name, out int n) ? n : int.MaxValue;
    }

    private static string MakeUniqueSheetName(string rawName, HashSet<string> usedNames)
    {
        string sanitized = new string(rawName.Where(c => !"[]*?/\\:".Contains(c)).ToArray());

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Building";
        }

        if (sanitized.Length > 31)
        {
            sanitized = sanitized[..31];
        }

        string candidate = sanitized;
        int suffix = 1;

        while (!usedNames.Add(candidate))
        {
            string suffixText = $" ({suffix})";
            int cut = Math.Max(0, 31 - suffixText.Length);
            candidate = sanitized[..Math.Min(cut, sanitized.Length)] + suffixText;
            suffix++;
        }

        return candidate;
    }

    private static byte[] BuildXlsx(string sheetName, string[] headers, List<string[]> rows)
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
        }

        if (rows.Count > 0)
        {
            worksheet.RangeUsed()?.SetAutoFilter();
        }

        worksheet.Columns().AdjustToContents();

        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}