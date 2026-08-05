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

    public async Task<UserPanelDashboardDto?> UpdateProfileAsync(UpdateUserPanelProfileRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/profile"
            : $"api/user-panel/profile?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto> CreateInvoiceAsync(CreateUserPanelInvoiceRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/invoices"
            : $"api/user-panel/invoices?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }

    public async Task CreateCompetitionEntryAsync(CreateCompetitionEntryRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/competition-entries"
            : $"api/user-panel/competition-entries?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be created.";
            throw new Exception(message);
        }
    }

    public async Task UpdateCompetitionEntryAsync(Guid id, UpdateCompetitionEntryRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/competition-entries/{id}"
            : $"api/user-panel/competition-entries/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be updated.";
            throw new Exception(message);
        }
    }

    public async Task DeleteCompetitionEntryAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/competition-entries/{id}"
            : $"api/user-panel/competition-entries/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be deleted.";
            throw new Exception(message);
        }
    }

    public async Task<MealPreferenceDto?> GetMealPreferenceAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/meal-preference"
            : $"api/user-panel/meal-preference?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetMealPreferenceResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferenceResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Preference;
    }

    public async Task<MealPreferenceDto?> SaveMealPreferenceAsync(SaveMealPreferenceRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/meal-preference"
            : $"api/user-panel/meal-preference?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<SaveMealPreferenceResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SaveMealPreferenceResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Meal preference could not be saved.";
            throw new Exception(message);
        }

        return response.Data?.Preference;
    }

    public async Task<int> GetEnabledFestivalModulesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/festival-modules"
            : $"api/user-panel/festival-modules?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>(url, cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/bus-reservations"
            : $"api/user-panel/bus-reservations?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusDto>> GetAvailableBusesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/available-buses"
            : $"api/user-panel/available-buses?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> CreateBusReservationsAsync(CreateBusReservationsRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/bus-reservations"
            : $"api/user-panel/bus-reservations?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetBusReservationsResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<BusReservationDto?> UpdateBusReservationAsync(Guid id, UpdateBusReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/bus-reservations/{id}"
            : $"api/user-panel/bus-reservations/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteBusReservationAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/bus-reservations/{id}"
            : $"api/user-panel/bus-reservations/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<DeleteBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be cancelled.";
            throw new Exception(message);
        }
    }

    public async Task<AccommodationReservationDto?> GetAccommodationReservationAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/accommodation-reservation"
            : $"api/user-panel/accommodation-reservation?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetAccommodationReservationResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Reservation;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetAvailableAccommodationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/available-accommodations"
            : $"api/user-panel/available-accommodations?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>(url, cancellationToken);

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

    public async Task<AccommodationReservationDto?> CreateAccommodationReservationAsync(CreateAccommodationReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/accommodation-reservation"
            : $"api/user-panel/accommodation-reservation?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task<AccommodationReservationDto?> UpdateAccommodationReservationAsync(Guid id, UpdateAccommodationReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/accommodation-reservation/{id}"
            : $"api/user-panel/accommodation-reservation/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteAccommodationReservationAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/accommodation-reservation/{id}"
            : $"api/user-panel/accommodation-reservation/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<DeleteAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be cancelled.";
            throw new Exception(message);
        }
    }
}