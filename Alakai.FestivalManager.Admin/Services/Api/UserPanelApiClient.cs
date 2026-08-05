namespace Alakai.FestivalManager.Admin.Services.Api;

public class UserPanelApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;

    public UserPanelApiClient(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<UserPanelDashboardDto?> GetDashboardAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/dashboard"
            : $"api/user-panel/dashboard?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetUserPanelDashboardResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto?> UpdateProfileAsync(UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/user-panel/profile", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto> CreateInvoiceAsync(CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/invoices", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }

    public async Task CreateCompetitionEntryAsync(CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/competition-entries", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be created.";
            throw new Exception(message);
        }
    }

    public async Task UpdateCompetitionEntryAsync(Guid id, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/competition-entries/{id}", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be updated.";
            throw new Exception(message);
        }
    }

    public async Task DeleteCompetitionEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/competition-entries/{id}", cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be deleted.";
            throw new Exception(message);
        }
    }

    public async Task<MealPreferenceDto?> GetMealPreferenceAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetMealPreferenceResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferenceResponse>>("api/user-panel/meal-preference", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Preference;
    }

    public async Task<MealPreferenceDto?> SaveMealPreferenceAsync(SaveMealPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/meal-preference", request, cancellationToken);

        ApiResponse<SaveMealPreferenceResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SaveMealPreferenceResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Meal preference could not be saved.";
            throw new Exception(message);
        }

        return response.Data?.Preference;
    }

    public async Task<int> GetEnabledFestivalModulesAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>("api/user-panel/festival-modules", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>("api/user-panel/bus-reservations", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusDto>> GetAvailableBusesAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>("api/user-panel/available-buses", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> CreateBusReservationsAsync(CreateBusReservationsRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/bus-reservations", request, cancellationToken);

        ApiResponse<GetBusReservationsResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<BusReservationDto?> UpdateBusReservationAsync(Guid id, UpdateBusReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/bus-reservations/{id}", request, cancellationToken);

        ApiResponse<CreateBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteBusReservationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/bus-reservations/{id}", cancellationToken);

        ApiResponse<DeleteBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be cancelled.";
            throw new Exception(message);
        }
    }

    public async Task<AccommodationReservationDto?> GetAccommodationReservationAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationReservationResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationResponse>>("api/user-panel/accommodation-reservation", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Reservation;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetAvailableAccommodationsAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>("api/user-panel/available-accommodations", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buildings ?? [];
    }

    public async Task<AccommodationBuildingDto?> GetAccommodationBuildingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationBuildingResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingResponse>>($"api/user-panel/accommodation-buildings/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Building;
    }

    public async Task<AccommodationReservationDto?> CreateAccommodationReservationAsync(CreateAccommodationReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/accommodation-reservation", request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task<AccommodationReservationDto?> UpdateAccommodationReservationAsync(Guid id, UpdateAccommodationReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/accommodation-reservation/{id}", request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteAccommodationReservationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/accommodation-reservation/{id}", cancellationToken);

        ApiResponse<DeleteAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be cancelled.";
            throw new Exception(message);
        }
    }
}