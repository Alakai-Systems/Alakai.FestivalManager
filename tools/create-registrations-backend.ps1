# Add-ProductionReports.ps1
#
# Pestana "Reports" dentro de Produccion. Reutiliza la infraestructura ya
# existente de informes (ReportsController + IReportService + BuildXlsx +
# ReportApiClient) - solo anade 5 metodos nuevos y una pantalla, nada de
# infraestructura nueva. Sin restricciones de rol nuevas (ReportsController
# ya no tenia [Authorize], igual que el resto de reports existentes).
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function New-CodeFile {
    param([string]$Path, [string]$Content, [string]$Description)
    if (Test-Path $Path) { Write-Host "SKIP (ya existe): $Path" -ForegroundColor Cyan; return $true }
    $directory = Split-Path -Path $Path -Parent
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    Set-Content -Path $Path -Value $Content -NoNewline
    Write-Host "OK: $Description -> $Path" -ForegroundColor Green
    return $true
}

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

# ---------------------------------------------------------------------------
# 1) IReportService: 5 metodos nuevos
# ---------------------------------------------------------------------------
$interfacePath = "Alakai.FestivalManager.Application/Features/Reports/Services/IReportService.cs"
$results += Patch-File -Path $interfacePath -Description "IReportService: 5 metodos de Produccion" -OldString @'
    Task<byte[]> GenerateMealsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
}
'@ -NewString @'
    Task<byte[]> GenerateMealsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionTeamReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionSuppliersReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionTripsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionItinerariesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default);
}
'@
if ($results -contains $false) { Write-Host "`nFallo (IReportService)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 2) ReportService: campos + constructor + 5 metodos nuevos
# ---------------------------------------------------------------------------
$servicePath = "Alakai.FestivalManager.Application/Features/Reports/Services/ReportService.cs"

$results += Patch-File -Path $servicePath -Description "ReportService: campos + constructor de repos de Produccion" -OldString @'
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
'@ -NewString @'
    private readonly IBusReservationRepository _busReservationRepository;
    private readonly IMealPreferenceRepository _mealPreferenceRepository;
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IProductionTripRepository _productionTripRepository;
    private readonly IRunnerItineraryRepository _runnerItineraryRepository;
    private readonly IProductionReservationRepository _productionReservationRepository;

    public ReportService(
        IRegistrationRepository registrationRepository,
        ICompetitionEntryRepository competitionEntryRepository,
        IAccommodationReservationRepository accommodationReservationRepository,
        IAccommodationBuildingRepository accommodationBuildingRepository,
        IBusReservationRepository busReservationRepository,
        IMealPreferenceRepository mealPreferenceRepository,
        IProductionPersonRepository productionPersonRepository,
        IProductionSupplierRepository productionSupplierRepository,
        IProductionTripRepository productionTripRepository,
        IRunnerItineraryRepository runnerItineraryRepository,
        IProductionReservationRepository productionReservationRepository)
    {
        _registrationRepository = registrationRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _accommodationBuildingRepository = accommodationBuildingRepository;
        _busReservationRepository = busReservationRepository;
        _mealPreferenceRepository = mealPreferenceRepository;
        _productionPersonRepository = productionPersonRepository;
        _productionSupplierRepository = productionSupplierRepository;
        _productionTripRepository = productionTripRepository;
        _runnerItineraryRepository = runnerItineraryRepository;
        _productionReservationRepository = productionReservationRepository;
    }
'@
if ($results -contains $false) { Write-Host "`nFallo (ReportService ctor)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $servicePath -Description "ReportService: 5 metodos de Produccion (antes de BuildXlsx)" -OldString @'
    private static byte[] BuildXlsx(string sheetName, string[] headers, List<string[]> rows)
'@ -NewString @'
    public async Task<byte[]> GenerateProductionTeamReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionPerson> people = await _productionPersonRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = people.Select(p => new[]
        {
            p.FirstName, p.LastName, p.Category.ToString(), p.RoleTitle, p.Email, p.Phone ?? "",
            p.DocumentType.ToString(), p.DocumentNumber, p.Nationality ?? ""
        }).ToList();

        return BuildXlsx("Production Team", ["First Name", "Last Name", "Category", "Role", "Email", "Phone", "Document Type", "Document Number", "Nationality"], rows);
    }

    public async Task<byte[]> GenerateProductionSuppliersReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionSupplier> suppliers = await _productionSupplierRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = suppliers.Select(s => new[]
        {
            s.Name, s.ServiceType, s.ContactName ?? "", s.Email ?? "", s.Phone ?? "", s.Notes ?? ""
        }).ToList();

        return BuildXlsx("Suppliers", ["Name", "Service Type", "Contact Name", "Email", "Phone", "Notes"], rows);
    }

    public async Task<byte[]> GenerateProductionTripsReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionTrip> trips = await _productionTripRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<string[]> rows = trips.Select(t => new[]
        {
            t.ProductionPerson is not null ? $"{t.ProductionPerson.FirstName} {t.ProductionPerson.LastName}" : "",
            t.Type.ToString(), t.TripNumber, t.DateTime.ToString("dd/MM/yyyy HH:mm"), t.TerminalOrStation, t.Direction.ToString()
        }).ToList();

        return BuildXlsx("Trips", ["Person", "Type", "Trip Number", "Date/Time", "Terminal / Station", "Direction"], rows);
    }

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

    public async Task<byte[]> GenerateProductionAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodationReservation> reservations = (await _productionReservationRepository.GetAllAsync(cancellationToken))
            .Where(r => r.EditionId == editionId)
            .ToList();

        List<string[]> rows = reservations.SelectMany(r => r.Occupants.Select(o => new[]
        {
            r.ProductionAccommodationBuilding?.Name ?? "",
            r.ResponsibleProductionPerson is not null ? $"{r.ResponsibleProductionPerson.FirstName} {r.ResponsibleProductionPerson.LastName}" : "",
            o.ProductionPerson is not null ? $"{o.ProductionPerson.FirstName} {o.ProductionPerson.LastName}" : "",
            o.ProductionAccommodation?.ProductionAccommodationZone?.Name ?? "",
            o.ProductionAccommodation?.Name ?? ""
        })).ToList();

        return BuildXlsx("Production Accommodation", ["Building", "Responsible", "Occupant", "Zone", "Room"], rows);
    }

    private static byte[] BuildXlsx(string sheetName, string[] headers, List<string[]> rows)
'@
if ($results -contains $false) { Write-Host "`nFallo (ReportService metodos)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 3) ReportsController: 5 casos nuevos en el switch
# ---------------------------------------------------------------------------
$controllerPath = "Alakai.FestivalManager.Api/Controllers/ReportsController.cs"

$results += Patch-File -Path $controllerPath -Description "ReportsController: 5 casos de Produccion" -OldString @'
            "meals" => await _reportService.GenerateMealsReportAsync(editionId, cancellationToken),
            _ => []
        };

        if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals"))
'@ -NewString @'
            "meals" => await _reportService.GenerateMealsReportAsync(editionId, cancellationToken),
            "production-team" => await _reportService.GenerateProductionTeamReportAsync(editionId, cancellationToken),
            "production-suppliers" => await _reportService.GenerateProductionSuppliersReportAsync(editionId, cancellationToken),
            "production-trips" => await _reportService.GenerateProductionTripsReportAsync(editionId, cancellationToken),
            "production-itineraries" => await _reportService.GenerateProductionItinerariesReportAsync(editionId, cancellationToken),
            "production-accommodation" => await _reportService.GenerateProductionAccommodationReportAsync(editionId, cancellationToken),
            _ => []
        };

        if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals" or "production-team" or "production-suppliers" or "production-trips" or "production-itineraries" or "production-accommodation"))
'@
if ($results -contains $false) { Write-Host "`nFallo (ReportsController)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 4) Pantalla ProductionReports.razor
# ---------------------------------------------------------------------------
$results += New-CodeFile -Path "Alakai.FestivalManager.Admin/Components/Pages/ProductionReports.razor" -Description "ProductionReports.razor" -Content @'
@page "/production-reports"

@inject ReportApiClient ReportApiClient
@inject EditionApiClient EditionApiClient
@inject FestivalApiClient FestivalApiClient
@inject ActiveFestivalState ActiveFestivalState
@inject IJSRuntime JsRuntime

<PageHeader Title="Production" pTitle="Reports"></PageHeader>

<div class="grid grid-cols-1 gap-4 min-h-[calc(100vh-212px)]">
    <div class="card">
        <div class="flex flex-col gap-3 md:flex-row md:items-center">
            <select class="form-select w-full md:w-56" @bind="selectedFestivalId" @bind:after="OnFestivalChangedAsync">
                @foreach (FestivalDto festival in festivals)
                {
                    <option value="@festival.Id">@festival.Name</option>
                }
            </select>
            <select class="form-select w-full md:w-64" @bind="selectedEditionId" @bind:after="OnEditionChangedAsync">
                @foreach (EditionDto edition in editionsForSelectedFestival)
                {
                    <option value="@edition.Id">@edition.Name</option>
                }
            </select>
        </div>
    </div>

    @if (!string.IsNullOrWhiteSpace(successMessage))
    {
        <div class="p-3 text-sm rounded bg-success/10 text-success">@successMessage</div>
    }
    @if (!string.IsNullOrWhiteSpace(errorMessage))
    {
        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
    }

    <div class="card">
        @if (selectedEditionId == Guid.Empty)
        {
            <p class="text-sm text-black/50 dark:text-white/60">Select a festival and edition above.</p>
        }
        else
        {
            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-4 py-3 font-semibold">Report</th>
                            <th class="px-4 py-3 font-semibold text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody class="text-black dark:text-white/80">
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Artists &amp; Team</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "production-team")" @onclick='() => DownloadAsync("production-team")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "production-team" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Suppliers</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "production-suppliers")" @onclick='() => DownloadAsync("production-suppliers")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "production-suppliers" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Accommodation</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "production-accommodation")" @onclick='() => DownloadAsync("production-accommodation")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "production-accommodation" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Trips</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "production-trips")" @onclick='() => DownloadAsync("production-trips")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "production-trips" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                        <tr class="border-b border-black/10 dark:border-darkborder">
                            <td class="px-4 py-3">Runner Itineraries</td>
                            <td class="px-4 py-3 text-right">
                                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@(downloadingReport == "production-itineraries")" @onclick='() => DownloadAsync("production-itineraries")'>
                                    <i class="ri-download-line ltr:mr-1 rtl:ml-1"></i>@(downloadingReport == "production-itineraries" ? "Downloading..." : "Download")
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        }
    </div>
</div>

<script>
    window.downloadFileFromBase64 = window.downloadFileFromBase64 || function (filename, base64, mimeType) {
        const byteCharacters = atob(base64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    };
</script>

@code {
    private const string XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private List<FestivalDto> festivals = [];
    private List<EditionDto> editions = [];
    private List<EditionDto> editionsForSelectedFestival = [];
    private Guid selectedFestivalId;
    private Guid selectedEditionId;
    private string? downloadingReport;
    private string? successMessage;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        festivals = (await FestivalApiClient.GetAllAsync()).ToList();
        editions = (await EditionApiClient.GetAllAsync()).ToList();

        selectedFestivalId = ActiveFestivalState.Active?.Id ?? festivals.FirstOrDefault()?.Id ?? Guid.Empty;
        UpdateEditionsForSelectedFestival();

        EditionDto? preferred = editionsForSelectedFestival.FirstOrDefault(e => e.IsActive) ?? editionsForSelectedFestival.FirstOrDefault();
        selectedEditionId = preferred?.Id ?? Guid.Empty;
    }

    private void UpdateEditionsForSelectedFestival()
    {
        editionsForSelectedFestival = editions.Where(e => e.FestivalId == selectedFestivalId).ToList();
    }

    private Task OnFestivalChangedAsync()
    {
        UpdateEditionsForSelectedFestival();
        selectedEditionId = editionsForSelectedFestival.FirstOrDefault()?.Id ?? Guid.Empty;
        return Task.CompletedTask;
    }

    private Task OnEditionChangedAsync()
    {
        return Task.CompletedTask;
    }

    private async Task DownloadAsync(string reportType)
    {
        if (selectedEditionId == Guid.Empty)
        {
            return;
        }

        downloadingReport = reportType;

        try
        {
            byte[] bytes = await ReportApiClient.GetReportXlsxAsync(reportType, selectedEditionId);
            string base64 = Convert.ToBase64String(bytes);
            await JsRuntime.InvokeVoidAsync("downloadFileFromBase64", $"{reportType}.xlsx", base64, XlsxMimeType);
            ShowSuccess($"{reportType} report downloaded successfully.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            downloadingReport = null;
        }
    }

    private void ShowSuccess(string message)
    {
        successMessage = message;
        errorMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            successMessage = null;
            StateHasChanged();
        });
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        successMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            errorMessage = null;
            StateHasChanged();
        });
    }
}
'@

if ($results -contains $false) { Write-Host "`nFallo (pagina)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 5) Sidebar: subitem Reports en el menu completo Y en el menu exclusivo de Production
# ---------------------------------------------------------------------------
$sidebarPath = "Alakai.FestivalManager.Admin/Components/Layout/Sidebar.razor"

$results += Patch-File -Path $sidebarPath -Description "Sidebar (menu completo): subitem Reports en Production" -OldString @'
                        <li><NavLink href="/production-itineraries">Runner Itineraries</NavLink></li>
                    </ul>
                </li>

                <li class="menu nav-item">
                    <NavLink href="/reports" class="text-black nav-link group">
'@ -NewString @'
                        <li><NavLink href="/production-itineraries">Runner Itineraries</NavLink></li>
                        <li><NavLink href="/production-reports">Reports</NavLink></li>
                    </ul>
                </li>

                <li class="menu nav-item">
                    <NavLink href="/reports" class="text-black nav-link group">
'@
if ($results -contains $false) { Write-Host "`nFallo (Sidebar menu completo)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $sidebarPath -Description "Sidebar (menu exclusivo Production): subitem Reports" -OldString @'
                    <li class="menu nav-item">
                        <NavLink href="/production-itineraries" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Runner Itineraries</span>
                            </div>
                        </NavLink>
                    </li>
                </ul>
            }
'@ -NewString @'
                    <li class="menu nav-item">
                        <NavLink href="/production-itineraries" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-team-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Runner Itineraries</span>
                            </div>
                        </NavLink>
                    </li>
                    <li class="menu nav-item">
                        <NavLink href="/production-reports" class="text-black nav-link group">
                            <div class="flex items-center">
                                <i class="ri-file-download-fill"></i>
                                <span class="ltr:pl-1.5 rtl:pr-1.5">Reports</span>
                            </div>
                        </NavLink>
                    </li>
                </ul>
            }
'@
if ($results -contains $false) { Write-Host "`nFallo (Sidebar menu Production)." -ForegroundColor Red; exit 1 }

Write-Host "`nPestana de Reports de Produccion completa: Artistas/Equipo, Proveedores, Alojamiento, Viajes, Itinerarios - los 5 descargables en xlsx." -ForegroundColor Green