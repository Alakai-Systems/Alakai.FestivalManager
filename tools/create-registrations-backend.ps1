# =====================================================================
# Alakai FestivalManager - User Panel: rediseno de la seccion de Buses
#
#   - Ida y vuelta en un solo formulario, un solo boton Save.
#   - Al guardar ambas (o una) se dispara un UNICO email de
#     confirmacion (CreateManyAsync en el backend).
#   - "Outbound" pasa a llamarse "Departure".
#   - En Departure, el origen es un link a Google Maps; en Return, el
#     destino lo es (donde te recogen yendo, donde te dejan volviendo).
#
# REQUIERE que ya tengas aplicado fix-userpanel-buses-only.ps1
# (reemplaza esa seccion, no la duplica).
#
# USO: desde la raiz del repo -> .\tools\userpanel-bus-redesign.ps1
# =====================================================================

$ErrorActionPreference = "Stop"
Write-Host "Trabajando en: $(Get-Location)" -ForegroundColor Cyan

$adminRoot = "Alakai.FestivalManager.Admin"

if (-not (Test-Path $adminRoot)) {
    Write-Host "ERROR: no se encontro la carpeta '$adminRoot'. Ejecuta este script desde la raiz del repo." -ForegroundColor Red
    exit 1
}

function Require-Path {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        Write-Host "ERROR: no se encontro $Path" -ForegroundColor Red
        exit 1
    }
}

function Patch-Normalized {
    param([string]$Path, [string]$OldText, [string]$NewText, [string]$Description)

    Require-Path $Path
    $raw = Get-Content $Path -Raw
    $content = $raw -replace "`r`n", "`n"
    $old = ($OldText -replace "`r`n", "`n").TrimEnd()
    $new = ($NewText -replace "`r`n", "`n").TrimEnd()

    if ($content.Contains($new)) {
        Write-Host "  Ya aplicado: $Description" -ForegroundColor Yellow
        return
    }

    $count = ([regex]::Matches($content, [regex]::Escape($old))).Count
    if ($count -ne 1) {
        Write-Host "ERROR: '$Description' -> se esperaba 1 coincidencia, se encontraron $count. No se modifico nada." -ForegroundColor Red
        Write-Host "Puede que tu archivo haya cambiado desde el ultimo script. Pegamelo de nuevo si sigue fallando." -ForegroundColor Yellow
        exit 1
    }

    $newContent = $content.Replace($old, $new)
    Set-Content -Path $Path -Value $newContent -Encoding UTF8 -NoNewline
    Write-Host "  Modificado: $Description" -ForegroundColor Green
}

$upPath = Join-Path $adminRoot "Components\Pages\UserPanelDashboard\UserPanel.razor"
Require-Path $upPath

Write-Host ""
$sectionOld = @'
<div class="card shadow-sm">
            <div class="mb-5" id="buses">
                <h2 class="text-lg font-bold text-black dark:text-white">Buses</h2>
            </div>

            @if (!string.IsNullOrWhiteSpace(BusSuccessMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@BusSuccessMessage</div>
            }
            @if (!string.IsNullOrWhiteSpace(BusErrorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@BusErrorMessage</div>
            }

            @if (IsLoadingBuses)
            {
                <p class="text-sm text-muted dark:text-darkmuted">Loading...</p>
            }
            else
            {
                <div class="space-y-6">
                    @foreach (int direction in new[] { 1, 2 })
                    {
                        BusReservationDto? existing = BusReservations.FirstOrDefault(r => r.Direction == direction);

                        <div>
                            <h3 class="mb-2 text-sm font-semibold text-black dark:text-white">@(direction == 1 ? "Outbound" : "Return")</h3>

                            @if (existing is not null)
                            {
                                <div class="flex items-center justify-between p-3 border rounded-md border-black/10 dark:border-darkborder">
                                    <div class="text-sm">
                                        <p class="text-black dark:text-white">@existing.DepartureTime.ToString("dd/MM/yyyy HH:mm")</p>
                                        <p class="text-muted dark:text-darkmuted">@existing.PickupLocation → @existing.DestinationLocation</p>
                                    </div>
                                    <div class="flex items-center gap-2">
                                        <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditBusModal(existing)"><i class="ri-pencil-line"></i></button>
                                        <button type="button" class="text-danger" title="Cancel" @onclick="() => OpenCancelBusModal(existing)"><i class="ri-delete-bin-line"></i></button>
                                    </div>
                                </div>
                            }
                            else
                            {
                                List<BusDto> options = AvailableBuses.Where(b => b.Direction == direction).ToList();

                                @if (options.Count == 0)
                                {
                                    <p class="text-sm text-muted dark:text-darkmuted">No @(direction == 1 ? "outbound" : "return") buses available for your pass right now.</p>
                                }
                                else
                                {
                                    <div class="flex flex-col gap-3 md:flex-row md:items-center">
                                        <select class="form-select" @bind="SelectedBusId[direction]">
                                            <option value="@Guid.Empty">-</option>
                                            @foreach (BusDto bus in options)
                                            {
                                                <option value="@bus.Id" disabled="@(bus.OccupiedCount >= bus.Capacity)">
                                                    @bus.DepartureTime.ToString("dd/MM HH:mm") - @bus.PickupLocation → @bus.DestinationLocation (@bus.OccupiedCount/@bus.Capacity)@(bus.OccupiedCount >= bus.Capacity ? " - FULL" : "")
                                                </option>
                                            }
                                        </select>
                                        <button type="button" class="transition-all duration-300 border rounded-md btn text-purple border-purple hover:bg-purple hover:text-white whitespace-nowrap disabled:opacity-50" disabled="@(!SelectedBusId.ContainsKey(direction) || SelectedBusId[direction] == Guid.Empty || IsSavingBus)" @onclick="() => BookBusAsync(direction)">
                                            @(IsSavingBus ? "Booking..." : "Book")
                                        </button>
                                    </div>
                                }
                            }
                        </div>
                    }
                </div>
            }
        </div>

        @if (editingBusReservation is not null)
        {
            <div class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 px-4 py-8">
                <div class="relative w-full overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder" style="width: min(95vw, 480px);">
                    <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                        <h3 class="text-lg font-semibold text-black dark:text-white">Edit Bus</h3>
                        <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseEditBusModal"><i class="ri-close-line text-2xl"></i></button>
                    </div>
                    <div class="p-5">
                        <label class="block mb-1 text-sm text-muted dark:text-darkmuted">Bus</label>
                        <select class="form-select" @bind="editBusNewId">
                            @foreach (BusDto bus in AvailableBuses.Where(b => b.Direction == editingBusReservation.Direction))
                            {
                                <option value="@bus.Id" disabled="@(bus.OccupiedCount >= bus.Capacity && bus.Id != editingBusReservation.BusId)">
                                    @bus.DepartureTime.ToString("dd/MM HH:mm") - @bus.PickupLocation → @bus.DestinationLocation (@bus.OccupiedCount/@bus.Capacity)
                                </option>
                            }
                        </select>
                    </div>
                    <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                        <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseEditBusModal">Cancel</button>
                        <button type="button" class="px-4 py-2 text-sm text-white rounded-md bg-purple disabled:opacity-50" disabled="@IsSavingBus" @onclick="SaveEditedBusAsync">@(IsSavingBus ? "Saving..." : "Save changes")</button>
                    </div>
                </div>
            </div>
        }

        @if (cancellingBusReservation is not null)
        {
            <div class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 px-4 py-8">
                <div class="relative w-[92vw] md:w-[420px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                    <div class="px-5 py-4">
                        <h3 class="text-lg font-semibold text-black dark:text-white">Cancel bus reservation</h3>
                        <p class="mt-2 text-sm text-muted dark:text-darkmuted">Are you sure you want to cancel this bus reservation?</p>
                    </div>
                    <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                        <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseCancelBusModal">Keep it</button>
                        <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@IsSavingBus" @onclick="CancelBusReservationAsync">
                            @(IsSavingBus ? "Cancelling..." : "Cancel reservation")
                        </button>
                    </div>
                </div>
            </div>
        }
'@
$sectionNew = @'
<div class="card shadow-sm">
            <div class="mb-5" id="buses">
                <h2 class="text-lg font-bold text-black dark:text-white">Buses</h2>
                <p class="mt-1 text-sm text-muted dark:text-darkmuted">Choose your departure and/or return bus, then save.</p>
            </div>

            @if (!string.IsNullOrWhiteSpace(BusSuccessMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@BusSuccessMessage</div>
            }
            @if (!string.IsNullOrWhiteSpace(BusErrorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@BusErrorMessage</div>
            }

            @if (IsLoadingBuses)
            {
                <p class="text-sm text-muted dark:text-darkmuted">Loading...</p>
            }
            else
            {
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                    @foreach (int direction in new[] { 1, 2 })
                    {
                        BusReservationDto? existing = BusReservations.FirstOrDefault(r => r.Direction == direction);

                        <div class="p-4 border rounded-lg border-black/10 dark:border-darkborder">
                            <div class="flex items-center gap-2 mb-3">
                                <i class="ri-bus-2-fill text-lg @(direction == 1 ? "text-purple" : "text-warning")"></i>
                                <h3 class="text-sm font-semibold text-black dark:text-white">@GetDirectionLabel(direction)</h3>
                            </div>

                            @if (existing is not null)
                            {
                                <div class="space-y-1 text-sm">
                                    <p class="text-black dark:text-white">@existing.DepartureTime.ToString("dd/MM/yyyy HH:mm")</p>
                                    @if (direction == 1)
                                    {
                                        <p class="text-muted dark:text-darkmuted">
                                            <a href="@GetMapsUrl(existing.PickupLocation)" target="_blank" class="text-purple hover:underline">@existing.PickupLocation</a>
                                            <i class="mx-1 ri-arrow-right-line"></i>@existing.DestinationLocation
                                        </p>
                                    }
                                    else
                                    {
                                        <p class="text-muted dark:text-darkmuted">
                                            @existing.PickupLocation<i class="mx-1 ri-arrow-right-line"></i>
                                            <a href="@GetMapsUrl(existing.DestinationLocation)" target="_blank" class="text-purple hover:underline">@existing.DestinationLocation</a>
                                        </p>
                                    }
                                </div>
                                <div class="flex items-center gap-3 mt-3">
                                    <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditBusModal(existing)"><i class="ri-pencil-line"></i></button>
                                    <button type="button" class="text-danger" title="Cancel" @onclick="() => OpenCancelBusModal(existing)"><i class="ri-delete-bin-line"></i></button>
                                </div>
                            }
                            else
                            {
                                List<BusDto> options = AvailableBuses.Where(b => b.Direction == direction).ToList();

                                @if (options.Count == 0)
                                {
                                    <p class="text-sm text-muted dark:text-darkmuted">No @(direction == 1 ? "departure" : "return") buses available for your pass right now.</p>
                                }
                                else
                                {
                                    <select class="form-select" @bind="PendingBusSelection[direction]">
                                        <option value="@Guid.Empty">-</option>
                                        @foreach (BusDto bus in options)
                                        {
                                            <option value="@bus.Id" disabled="@(bus.OccupiedCount >= bus.Capacity)">
                                                @bus.DepartureTime.ToString("dd/MM HH:mm") - @bus.PickupLocation → @bus.DestinationLocation (@bus.OccupiedCount/@bus.Capacity)@(bus.OccupiedCount >= bus.Capacity ? " - FULL" : "")
                                            </option>
                                        }
                                    </select>
                                }
                            }
                        </div>
                    }
                </div>

                @if (HasPendingBusSelection)
                {
                    <div class="flex justify-end mt-4">
                        <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85] disabled:opacity-50" disabled="@IsSavingBus" @onclick="SaveBusSelectionAsync">
                            @(IsSavingBus ? "Saving..." : "Save")
                        </button>
                    </div>
                }
            }
        </div>

        @if (editingBusReservation is not null)
        {
            <div class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 px-4 py-8">
                <div class="relative w-full overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder" style="width: min(95vw, 480px);">
                    <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                        <h3 class="text-lg font-semibold text-black dark:text-white">Edit Bus</h3>
                        <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseEditBusModal"><i class="ri-close-line text-2xl"></i></button>
                    </div>
                    <div class="p-5">
                        <label class="block mb-1 text-sm text-muted dark:text-darkmuted">Bus</label>
                        <select class="form-select" @bind="editBusNewId">
                            @foreach (BusDto bus in AvailableBuses.Where(b => b.Direction == editingBusReservation.Direction))
                            {
                                <option value="@bus.Id" disabled="@(bus.OccupiedCount >= bus.Capacity && bus.Id != editingBusReservation.BusId)">
                                    @bus.DepartureTime.ToString("dd/MM HH:mm") - @bus.PickupLocation → @bus.DestinationLocation (@bus.OccupiedCount/@bus.Capacity)
                                </option>
                            }
                        </select>
                    </div>
                    <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                        <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseEditBusModal">Cancel</button>
                        <button type="button" class="px-4 py-2 text-sm text-white rounded-md bg-purple disabled:opacity-50" disabled="@IsSavingBus" @onclick="SaveEditedBusAsync">@(IsSavingBus ? "Saving..." : "Save changes")</button>
                    </div>
                </div>
            </div>
        }

        @if (cancellingBusReservation is not null)
        {
            <div class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 px-4 py-8">
                <div class="relative w-[92vw] md:w-[420px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                    <div class="px-5 py-4">
                        <h3 class="text-lg font-semibold text-black dark:text-white">Cancel bus reservation</h3>
                        <p class="mt-2 text-sm text-muted dark:text-darkmuted">Are you sure you want to cancel this bus reservation?</p>
                    </div>
                    <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                        <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseCancelBusModal">Keep it</button>
                        <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@IsSavingBus" @onclick="CancelBusReservationAsync">
                            @(IsSavingBus ? "Cancelling..." : "Cancel reservation")
                        </button>
                    </div>
                </div>
            </div>
        }
'@
Patch-Normalized -Path $upPath -OldText $sectionOld -NewText $sectionNew -Description "seccion de Buses (markup)"

$codeOld = @'
    // ==================== Buses ====================
    private const int TransportModuleFlag = 4;
    private bool HasTransportModule => (EnabledFestivalModules & TransportModuleFlag) != 0;

    private List<BusDto> AvailableBuses { get; set; } = [];
    private List<BusReservationDto> BusReservations { get; set; } = [];
    private Dictionary<int, Guid> SelectedBusId { get; set; } = new() { [1] = Guid.Empty, [2] = Guid.Empty };
    private bool IsLoadingBuses { get; set; } = true;
    private bool IsSavingBus { get; set; }
    private string? BusSuccessMessage { get; set; }
    private string? BusErrorMessage { get; set; }

    private BusReservationDto? editingBusReservation;
    private Guid editBusNewId;
    private BusReservationDto? cancellingBusReservation;

    private void ShowBusSuccess(string message)
    {
        BusSuccessMessage = message;
        BusErrorMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            BusSuccessMessage = null;
            StateHasChanged();
        });
    }

    private void ShowBusError(string message)
    {
        BusErrorMessage = message;
        BusSuccessMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            BusErrorMessage = null;
            StateHasChanged();
        });
    }

    private async Task LoadBusDataAsync()
    {
        if (Dashboard?.Registration is null)
        {
            IsLoadingBuses = false;
            return;
        }

        IsLoadingBuses = true;

        try
        {
            BusReservations = (await BusApiClient.GetReservationsByRegistrationAsync(Dashboard.Registration.Id)).ToList();
            AvailableBuses = (await BusApiClient.GetAvailableForRegistrationAsync(Dashboard.Registration.Id)).ToList();
        }
        catch
        {
            // Non-critical: the buses section just stays empty if this fails.
        }
        finally
        {
            IsLoadingBuses = false;
        }
    }

    private async Task BookBusAsync(int direction)
    {
        if (Dashboard?.Registration is null || !SelectedBusId.TryGetValue(direction, out Guid busId) || busId == Guid.Empty)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            CreateBusReservationRequest request = new()
            {
                BusId = busId,
                RegistrationId = Dashboard.Registration.Id
            };

            await BusApiClient.CreateReservationAsync(request);
            ShowBusSuccess("Bus reservation created successfully.");
            SelectedBusId[direction] = Guid.Empty;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }

    private void OpenEditBusModal(BusReservationDto reservation)
    {
        editingBusReservation = reservation;
        editBusNewId = reservation.BusId;
    }

    private void CloseEditBusModal()
    {
        editingBusReservation = null;
    }

    private async Task SaveEditedBusAsync()
    {
        if (editingBusReservation is null || Dashboard?.Registration is null)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            UpdateBusReservationRequest request = new()
            {
                NewBusId = editBusNewId,
                RequestingRegistrationId = Dashboard.Registration.Id
            };

            await BusApiClient.UpdateReservationAsync(editingBusReservation.Id, request, isAdmin: false);
            ShowBusSuccess("Bus reservation updated successfully.");
            editingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }

    private void OpenCancelBusModal(BusReservationDto reservation)
    {
        cancellingBusReservation = reservation;
    }

    private void CloseCancelBusModal()
    {
        cancellingBusReservation = null;
    }

    private async Task CancelBusReservationAsync()
    {
        if (cancellingBusReservation is null || Dashboard?.Registration is null)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            await BusApiClient.DeleteReservationAsync(cancellingBusReservation.Id, Dashboard.Registration.Id, isAdmin: false);
            ShowBusSuccess("Bus reservation cancelled successfully.");
            cancellingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@
$codeNew = @'
    // ==================== Buses ====================
    private const int TransportModuleFlag = 4;
    private bool HasTransportModule => (EnabledFestivalModules & TransportModuleFlag) != 0;

    private List<BusDto> AvailableBuses { get; set; } = [];
    private List<BusReservationDto> BusReservations { get; set; } = [];
    private Dictionary<int, Guid> PendingBusSelection { get; set; } = new() { [1] = Guid.Empty, [2] = Guid.Empty };
    private bool IsLoadingBuses { get; set; } = true;
    private bool IsSavingBus { get; set; }
    private string? BusSuccessMessage { get; set; }
    private string? BusErrorMessage { get; set; }

    private BusReservationDto? editingBusReservation;
    private Guid editBusNewId;
    private BusReservationDto? cancellingBusReservation;

    private bool HasPendingBusSelection =>
        PendingBusSelection.Any(kv => kv.Value != Guid.Empty && !BusReservations.Any(r => r.Direction == kv.Key));

    private static string GetDirectionLabel(int direction) => direction switch
    {
        1 => "Departure",
        2 => "Return",
        _ => "-"
    };

    private static string GetMapsUrl(string location)
    {
        return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(location)}";
    }

    private void ShowBusSuccess(string message)
    {
        BusSuccessMessage = message;
        BusErrorMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            BusSuccessMessage = null;
            StateHasChanged();
        });
    }

    private void ShowBusError(string message)
    {
        BusErrorMessage = message;
        BusSuccessMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            BusErrorMessage = null;
            StateHasChanged();
        });
    }

    private async Task LoadBusDataAsync()
    {
        if (Dashboard?.Registration is null)
        {
            IsLoadingBuses = false;
            return;
        }

        IsLoadingBuses = true;

        try
        {
            BusReservations = (await BusApiClient.GetReservationsByRegistrationAsync(Dashboard.Registration.Id)).ToList();
            AvailableBuses = (await BusApiClient.GetAvailableForRegistrationAsync(Dashboard.Registration.Id)).ToList();
            PendingBusSelection = new() { [1] = Guid.Empty, [2] = Guid.Empty };
        }
        catch
        {
            // Non-critical: the buses section just stays empty if this fails.
        }
        finally
        {
            IsLoadingBuses = false;
        }
    }

    private async Task SaveBusSelectionAsync()
    {
        if (Dashboard?.Registration is null)
        {
            return;
        }

        List<Guid> busIds = PendingBusSelection
            .Where(kv => kv.Value != Guid.Empty && !BusReservations.Any(r => r.Direction == kv.Key))
            .Select(kv => kv.Value)
            .ToList();

        if (busIds.Count == 0)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            CreateBusReservationsRequest request = new()
            {
                RegistrationId = Dashboard.Registration.Id,
                BusIds = busIds
            };

            await BusApiClient.CreateReservationsAsync(request);
            ShowBusSuccess(busIds.Count > 1 ? "Bus reservations created successfully." : "Bus reservation created successfully.");
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }

    private void OpenEditBusModal(BusReservationDto reservation)
    {
        editingBusReservation = reservation;
        editBusNewId = reservation.BusId;
    }

    private void CloseEditBusModal()
    {
        editingBusReservation = null;
    }

    private async Task SaveEditedBusAsync()
    {
        if (editingBusReservation is null || Dashboard?.Registration is null)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            UpdateBusReservationRequest request = new()
            {
                NewBusId = editBusNewId,
                RequestingRegistrationId = Dashboard.Registration.Id
            };

            await BusApiClient.UpdateReservationAsync(editingBusReservation.Id, request, isAdmin: false);
            ShowBusSuccess("Bus reservation updated successfully.");
            editingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }

    private void OpenCancelBusModal(BusReservationDto reservation)
    {
        cancellingBusReservation = reservation;
    }

    private void CloseCancelBusModal()
    {
        cancellingBusReservation = null;
    }

    private async Task CancelBusReservationAsync()
    {
        if (cancellingBusReservation is null || Dashboard?.Registration is null)
        {
            return;
        }

        IsSavingBus = true;

        try
        {
            await BusApiClient.DeleteReservationAsync(cancellingBusReservation.Id, Dashboard.Registration.Id, isAdmin: false);
            ShowBusSuccess("Bus reservation cancelled successfully.");
            cancellingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@
Patch-Normalized -Path $upPath -OldText $codeOld -NewText $codeNew -Description "codigo de Buses"

Write-Host ""
Write-Host "Completado. Reinicia el Admin." -ForegroundColor Cyan