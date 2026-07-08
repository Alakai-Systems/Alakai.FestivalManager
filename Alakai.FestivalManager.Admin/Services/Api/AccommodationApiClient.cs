namespace Alakai.FestivalManager.Admin.Services.Api;

public class AccommodationApiClient
{
    private readonly HttpClient _httpClient;

    public AccommodationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetBuildingsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>($"api/accommodation-buildings?editionId={editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load accommodation buildings.", response?.Errors);
        }

        return response.Data?.Buildings ?? [];
    }

    public async Task<AccommodationBuildingDto> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetAccommodationBuildingResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingResponse>>($"api/accommodation-buildings/{id}", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load the building.", response?.Errors);
        }

        return response.Data.Building;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>($"api/accommodation-buildings/available-for-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load available accommodations.", response?.Errors);
        }

        return response.Data?.Buildings ?? [];
    }

    public async Task<AccommodationBuildingDto> CreateBuildingAsync(CreateAccommodationBuildingRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/accommodation-buildings", request, cancellationToken);
        ApiResponse<CreateAccommodationBuildingResponse>? response = await ReadResponseAsync<CreateAccommodationBuildingResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Building;
    }

    public async Task UpdateBuildingAsync(Guid id, UpdateAccommodationBuildingRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/accommodation-buildings/{id}", request, cancellationToken);
        ApiResponse<UpdateAccommodationBuildingResponse>? response = await ReadResponseAsync<UpdateAccommodationBuildingResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteBuildingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/accommodation-buildings/{id}", cancellationToken);
        ApiResponse<DeleteAccommodationEntityResponse>? response = await ReadResponseAsync<DeleteAccommodationEntityResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task CreateZoneAsync(CreateAccommodationZoneRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/accommodation-zones", request, cancellationToken);
        ApiResponse<CreateAccommodationZoneResponse>? response = await ReadResponseAsync<CreateAccommodationZoneResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateZoneAsync(Guid id, UpdateAccommodationZoneRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/accommodation-zones/{id}", request, cancellationToken);
        ApiResponse<UpdateAccommodationZoneResponse>? response = await ReadResponseAsync<UpdateAccommodationZoneResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteZoneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/accommodation-zones/{id}", cancellationToken);
        ApiResponse<DeleteAccommodationEntityResponse>? response = await ReadResponseAsync<DeleteAccommodationEntityResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task CreateAccommodationsAsync(CreateAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/accommodations", request, cancellationToken);
        ApiResponse<CreateAccommodationResponse>? response = await ReadResponseAsync<CreateAccommodationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAccommodationAsync(Guid id, UpdateAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/accommodations/{id}", request, cancellationToken);
        ApiResponse<UpdateAccommodationResponse>? response = await ReadResponseAsync<UpdateAccommodationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAccommodationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/accommodations/{id}", cancellationToken);
        ApiResponse<DeleteAccommodationEntityResponse>? response = await ReadResponseAsync<DeleteAccommodationEntityResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    private static async Task<ApiResponse<T>?> ReadResponseAsync<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        try
        {
            return await httpResponse.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken);
        }
        catch (JsonException)
        {
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            string message = string.IsNullOrWhiteSpace(content) ? $"Request failed with status code {(int)httpResponse.StatusCode}." : content;
            throw new ApiClientException(message);
        }
    }

    private static void EnsureSuccess<T>(HttpResponseMessage httpResponse, ApiResponse<T>? response)
    {
        if (httpResponse.IsSuccessStatusCode && response?.Success == true)
        {
            return;
        }

        string message = response?.Message ?? $"Request failed with status code {(int)httpResponse.StatusCode}.";
        IReadOnlyList<string> errors = response?.Errors ?? [];
        throw new ApiClientException(message, errors);
    }

    public async Task<AccommodationReservationDto> CreateReservationAsync(CreateAccommodationReservationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/accommodation-reservations", request, cancellationToken);
        ApiResponse<CreateAccommodationReservationResponse>? response = await ReadResponseAsync<CreateAccommodationReservationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Reservation;
    }

    public async Task<AccommodationReservationDto?> GetReservationByRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetAccommodationReservationResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationResponse>>($"api/accommodation-reservations/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load the reservation.", response?.Errors);
        }

        return response.Data?.Reservation;
    }

    public async Task<IReadOnlyList<AccommodationReservationDto>> GetReservationsByBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetAccommodationReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationsResponse>>($"api/accommodation-reservations/by-building/{buildingId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load reservations.", response?.Errors);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task DeleteReservationAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/accommodation-reservations/{id}?requestingRegistrationId={requestingRegistrationId}&isAdmin={isAdmin}", cancellationToken);
        ApiResponse<DeleteAccommodationReservationResponse>? response = await ReadResponseAsync<DeleteAccommodationReservationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task<AccommodationReservationDto> UpdateReservationAsync(Guid id, UpdateAccommodationReservationRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/accommodation-reservations/{id}?isAdmin={isAdmin}", request, cancellationToken);
        ApiResponse<CreateAccommodationReservationResponse>? response = await ReadResponseAsync<CreateAccommodationReservationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Reservation;
    }
}