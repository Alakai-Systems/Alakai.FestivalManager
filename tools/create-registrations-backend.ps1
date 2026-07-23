# Fix-Step78-ImpersonationUI.ps1
# Parte 2: cliente del Admin, pagina de aterrizaje, y botones en Registros y
# Alojamiento (junto al nombre de cada ocupante que tenga cuenta propia).
# Solo visible/funcional para SuperAdmin.
#
# Ejecutar DESPUES de Fix-Step77-ImpersonationBackend.ps1.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

function Patch-File {
    param(
        [string]$Path,
        [string]$OldString,
        [string]$NewString,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return $false
    }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"

    if ($normalizedContent.Contains($normalizedNew)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)

    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

function New-FileIfMissing {
    param([string]$Path, [string]$Content, [string]$Description)

    if (Test-Path $Path) {
        Write-Host "SKIP (ya existe): $Description" -ForegroundColor Cyan
        return $true
    }

    $directory = Split-Path -Path $Path -Parent
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -Path $Path -Value $Content -NoNewline
    Write-Host "OK (creado): $Description" -ForegroundColor Green
    return $true
}

$results = @()

# ── 1. Admin: cliente de impersonacion ──────────────────────────────────────
$results += New-FileIfMissing -Path "Alakai.FestivalManager.Admin/Services/Api/ImpersonationApiClient.cs" -Description "ImpersonationApiClient" -Content @'
namespace Alakai.FestivalManager.Admin.Services.Api;

public class ImpersonationApiClient
{
    private readonly HttpClient _httpClient;

    public ImpersonationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetTokenForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/impersonation/by-registration/{registrationId}", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not start impersonation: {body}", null);
        }

        ImpersonationTokenResult? result = await response.Content.ReadFromJsonAsync<ImpersonationTokenResult>(cancellationToken: cancellationToken);
        return result?.AccessToken;
    }
}

public class ImpersonationTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
}
'@

# ── 2. Pagina de aterrizaje: /impersonate ───────────────────────────────────
$results += New-FileIfMissing -Path "Alakai.FestivalManager.Admin/Components/Pages/Impersonate.razor" -Description "Pagina Impersonate.razor" -Content @'
@page "/impersonate"
@inject Alakai.FestivalManager.Admin.Services.Auth.ITokenStorageService TokenStorageService
@inject NavigationManager Navigation

<div class="flex items-center justify-center min-h-screen">
    <p class="text-sm text-black/60 dark:text-white/60">Signing you in...</p>
</div>

@code {
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(Token))
        {
            await TokenStorageService.SetTokenAsync(Token);
        }

        Navigation.NavigateTo("/user-panel/dashboard", forceLoad: true);
    }
}
'@

if ($results -contains $false) {
    Write-Host "`nAlgun archivo nuevo no se pudo crear. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 3. Registrations.razor ───────────────────────────────────────────────────
$regPath = "Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor"
$results2 = @()

$results2 += Patch-File -Path $regPath -Description "Registrations: inyecciones necesarias" -OldString @'
@inject ActiveFestivalState ActiveFestivalState
'@ -NewString @'
@inject ActiveFestivalState ActiveFestivalState
@inject Alakai.FestivalManager.Admin.Services.Api.ImpersonationApiClient ImpersonationApiClient
@inject IJSRuntime JsRuntime
@using Microsoft.AspNetCore.Components.Authorization
'@

$results2 += Patch-File -Path $regPath -Description "Registrations: boton de impersonar junto a Edit/Delete" -OldString @'
                                            <button type="button" class="text-black dark:text-white/80 hover:text-purple" title="Edit" @onclick="() => OpenEditModal(registration)"><i class="ri-pencil-line"></i></button>
                                            <button type="button" class="text-danger hover:text-danger/70" title="Delete and send cancellation email" @onclick="() => OpenDeleteModal(registration)"><i class="ri-delete-bin-line"></i></button>
'@ -NewString @'
                                            @if (IsSuperAdmin)
                                            {
                                                <button type="button" class="text-black dark:text-white/80 hover:text-purple" title="View user panel" @onclick="() => ImpersonateAsync(registration.Id)"><i class="ri-key-2-line"></i></button>
                                            }
                                            <button type="button" class="text-black dark:text-white/80 hover:text-purple" title="Edit" @onclick="() => OpenEditModal(registration)"><i class="ri-pencil-line"></i></button>
                                            <button type="button" class="text-danger hover:text-danger/70" title="Delete and send cancellation email" @onclick="() => OpenDeleteModal(registration)"><i class="ri-delete-bin-line"></i></button>
'@

$results2 += Patch-File -Path $regPath -Description "Registrations: estado y metodo de impersonacion" -OldString @'
@code {
    private List<RegistrationDto> registrations = [];
'@ -NewString @'
@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool IsSuperAdmin { get; set; }

    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private List<RegistrationDto> registrations = [];
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Registrations.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

$resultRegInit = Patch-File -Path $regPath -Description "Registrations: comprobar SuperAdmin en OnInitializedAsync" -OldString @'
    protected override async Task OnInitializedAsync()
    {
'@ -NewString @'
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            AuthenticationState authState = await AuthenticationStateTask;
            IsSuperAdmin = authState.User.IsInRole("SuperAdmin");
        }

'@

if (-not $resultRegInit) {
    Write-Host "`nNo se pudo aplicar el patch de OnInitializedAsync en Registrations.razor - puede que el nombre del metodo de inicio difiera. Pegame ese metodo para localizarlo a mano." -ForegroundColor Yellow
}

Write-Host "`nRegistrations.razor completo." -ForegroundColor Green
Write-Host "Siguiente: AccommodationOperations.razor" -ForegroundColor Cyan

# ── 4. AccommodationOperations.razor ─────────────────────────────────────────
$accPath = "Alakai.FestivalManager.Admin/Components/Pages/AccommodationOperations.razor"
$results3 = @()

$results3 += Patch-File -Path $accPath -Description "Accommodation: inyecciones necesarias" -OldString @'
@inject ActiveFestivalState ActiveFestivalState
'@ -NewString @'
@inject ActiveFestivalState ActiveFestivalState
@inject Alakai.FestivalManager.Admin.Services.Api.ImpersonationApiClient ImpersonationApiClient
@inject IJSRuntime JsRuntime
@using Microsoft.AspNetCore.Components.Authorization
'@

$results3 += Patch-File -Path $accPath -Description "Accommodation: boton de impersonar junto al nombre del ocupante" -OldString @'
                                            <div class="flex items-center justify-between gap-2 px-3 py-2 text-xs border-t border-black/5 dark:border-white/5 @(occupant.IsResponsible ? "bg-black/5 dark:bg-white/5 font-semibold" : "")">
                                                <span class="truncate">@GetDisplayName(occupant)</span>
                                                @if (occupant.IsResponsible)
                                                {
                                                    <div class="flex items-center gap-2 shrink-0">
                                                        <button type="button" class="text-black dark:text-white/80" title="Edit reservation" @onclick="() => OpenEditModal(occupant)"><i class="ri-pencil-line"></i></button>
                                                        <button type="button" class="text-danger" title="Cancel reservation" @onclick="() => OpenCancelModal(occupant)"><i class="ri-delete-bin-line"></i></button>
                                                    </div>
                                                }
                                            </div>
'@ -NewString @'
                                            <div class="flex items-center justify-between gap-2 px-3 py-2 text-xs border-t border-black/5 dark:border-white/5 @(occupant.IsResponsible ? "bg-black/5 dark:bg-white/5 font-semibold" : "")">
                                                <span class="truncate">@GetDisplayName(occupant)</span>
                                                <div class="flex items-center gap-2 shrink-0">
                                                    @if (IsSuperAdmin && occupant.RegistrationId.HasValue)
                                                    {
                                                        <button type="button" class="text-black dark:text-white/80" title="View user panel" @onclick="() => ImpersonateAsync(occupant.RegistrationId.Value)"><i class="ri-key-2-line"></i></button>
                                                    }
                                                    @if (occupant.IsResponsible)
                                                    {
                                                        <button type="button" class="text-black dark:text-white/80" title="Edit reservation" @onclick="() => OpenEditModal(occupant)"><i class="ri-pencil-line"></i></button>
                                                        <button type="button" class="text-danger" title="Cancel reservation" @onclick="() => OpenCancelModal(occupant)"><i class="ri-delete-bin-line"></i></button>
                                                    }
                                                </div>
                                            </div>
'@

$results3 += Patch-File -Path $accPath -Description "Accommodation: estado y metodo de impersonacion" -OldString @'
@code {
    private List<FestivalDto> festivals = [];
'@ -NewString @'
@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool IsSuperAdmin { get; set; }
    private string? errorMessage;

    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private List<FestivalDto> festivals = [];
'@

if ($results3 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (AccommodationOperations.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

$resultAccInit = Patch-File -Path $accPath -Description "Accommodation: comprobar SuperAdmin en OnInitializedAsync" -OldString @'
    protected override async Task OnInitializedAsync()
    {
'@ -NewString @'
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            AuthenticationState authState = await AuthenticationStateTask;
            IsSuperAdmin = authState.User.IsInRole("SuperAdmin");
        }

'@

if (-not $resultAccInit) {
    Write-Host "`nNo se pudo aplicar el patch de OnInitializedAsync en AccommodationOperations.razor - puede que el nombre del metodo de inicio difiera. Pegame ese metodo para localizarlo a mano." -ForegroundColor Yellow
}

Write-Host "`nTodo completo. dotnet build para confirmar." -ForegroundColor Green
Write-Host "El boton solo aparece para SuperAdmin, y en Alojamiento solo para ocupantes que tengan cuenta propia (con Registration asociado)." -ForegroundColor Cyan