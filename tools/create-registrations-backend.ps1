# Fix-ItinerarySequenceNumbers.ps1
#
# 1. Runner Itineraries: primera columna nueva "#" con el numero de orden
#    cronologico (1 = el mas cercano en fecha/hora, calculado sobre TODOS
#    los itinerarios de la edicion, no solo la pagina visible).
# 2. Trips: en la columna Itinerary, en vez de "Assigned"/"-" ahora sale
#    "Itinerary X" con ese mismo numero, o "-" si no esta asignado a
#    ninguno.
#
# No hace falta migracion - el numero se calcula al vuelo ordenando por
# fecha, no se guarda en ningun sitio.
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

# ===========================================================================
# PARTE 1: Runner Itineraries - columna "#" de orden cronologico
# ===========================================================================
$itinPath = "Alakai.FestivalManager.Admin/Components/Pages/RunnerItineraries.razor"

$results += Patch-File -Path $itinPath -Description "Itineraries: cabecera # (primera columna)" -OldString @'
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("DateTime")'>Date/Time @SortIcon("DateTime")</button></th>
'@ -NewString @'
                            <th class="px-4 py-3 font-semibold">#</th>
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("DateTime")'>Date/Time @SortIcon("DateTime")</button></th>
'@
if ($results -contains $false) { Write-Host "`nFallo (cabecera)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $itinPath -Description "Itineraries: colspan 6 -> 7" -OldString @'
                                <td colspan="6" class="px-4 py-6 text-center text-black/50 dark:text-white/60">No itineraries found.</td>
'@ -NewString @'
                                <td colspan="7" class="px-4 py-6 text-center text-black/50 dark:text-white/60">No itineraries found.</td>
'@
if ($results -contains $false) { Write-Host "`nFallo (colspan)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $itinPath -Description "Itineraries: celda # en la fila" -OldString @'
                                    <td class="px-4 py-3 font-medium">@itinerary.DateTime.ToString("dd MMM yyyy HH:mm")</td>
'@ -NewString @'
                                    <td class="px-4 py-3 font-medium">@ItinerarySequenceNumbers.GetValueOrDefault(itinerary.Id)</td>
                                    <td class="px-4 py-3">@itinerary.DateTime.ToString("dd MMM yyyy HH:mm")</td>
'@
if ($results -contains $false) { Write-Host "`nFallo (celda)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $itinPath -Description "Itineraries: metodo ItinerarySequenceNumbers" -OldString @'
    private List<string> DistinctRunners =>
'@ -NewString @'
    private Dictionary<Guid, int> ItinerarySequenceNumbers =>
        itineraries
            .OrderBy(i => i.DateTime)
            .Select((i, index) => (i.Id, Number: index + 1))
            .ToDictionary(x => x.Id, x => x.Number);

    private List<string> DistinctRunners =>
'@
if ($results -contains $false) { Write-Host "`nFallo (metodo numeracion)." -ForegroundColor Red; exit 1 }

Write-Host "`nRunner Itineraries: columna # anadida, ordenada por fecha/hora." -ForegroundColor Green

# ===========================================================================
# PARTE 2: Trips - "Itinerary X" en vez de "Assigned"
# ===========================================================================
$tripsPath = "Alakai.FestivalManager.Admin/Components/Pages/ProductionTrips.razor"

$results += Patch-File -Path $tripsPath -Description "Trips: inyectar RunnerItineraryApiClient" -OldString @'
@inject ProductionTripApiClient ProductionTripApiClient
@inject ProductionPersonApiClient ProductionPersonApiClient
'@ -NewString @'
@inject ProductionTripApiClient ProductionTripApiClient
@inject RunnerItineraryApiClient RunnerItineraryApiClient
@inject ProductionPersonApiClient ProductionPersonApiClient
'@
if ($results -contains $false) { Write-Host "`nFallo (inject)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $tripsPath -Description "Trips: cargar itinerarios de la edicion" -OldString @'
            trips = targetEditionId != Guid.Empty
                ? (await ProductionTripApiClient.GetByEditionIdAsync(targetEditionId)).ToList()
                : [];
'@ -NewString @'
            trips = targetEditionId != Guid.Empty
                ? (await ProductionTripApiClient.GetByEditionIdAsync(targetEditionId)).ToList()
                : [];

            itineraries = targetEditionId != Guid.Empty
                ? (await RunnerItineraryApiClient.GetByEditionIdAsync(targetEditionId)).ToList()
                : [];
'@
if ($results -contains $false) { Write-Host "`nFallo (carga)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $tripsPath -Description "Trips: campo itineraries" -OldString @'
    private List<ProductionTripDto> trips = [];
'@ -NewString @'
    private List<ProductionTripDto> trips = [];
    private List<RunnerItineraryDto> itineraries = [];
'@
if ($results -contains $false) { Write-Host "`nFallo (campo)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $tripsPath -Description "Trips: metodo ItinerarySequenceNumbers" -OldString @'
    private async Task LoadDataAsync()
'@ -NewString @'
    private Dictionary<Guid, int> ItinerarySequenceNumbers =>
        itineraries
            .OrderBy(i => i.DateTime)
            .Select((i, index) => (i.Id, Number: index + 1))
            .ToDictionary(x => x.Id, x => x.Number);

    private async Task LoadDataAsync()
'@
if ($results -contains $false) { Write-Host "`nFallo (metodo numeracion)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $tripsPath -Description "Trips: mostrar 'Itinerary X' en vez de Assigned" -OldString @'
                                    <td class="px-4 py-3">@(trip.RunnerItineraryId.HasValue ? "Assigned" : "-")</td>
'@ -NewString @'
                                    <td class="px-4 py-3">@(trip.RunnerItineraryId.HasValue ? $"Itinerary {ItinerarySequenceNumbers.GetValueOrDefault(trip.RunnerItineraryId.Value)}" : "-")</td>
'@
if ($results -contains $false) { Write-Host "`nFallo (celda Itinerary)." -ForegroundColor Red; exit 1 }

Write-Host "`nTrips: columna Itinerary ahora muestra 'Itinerary X' con el mismo numero que la pantalla de Runner Itineraries." -ForegroundColor Green