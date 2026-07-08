namespace Alakai.FestivalManager.Admin.Services.Api;

public class BusApiClient
{
    private readonly HttpClient _httpClient;

    public BusApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<BusDto>> GetAllAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>($"api/buses?editionId={editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load buses.", response?.Errors);
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<IReadOnlyList<BusDto>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>($"api/buses/available-for-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load available buses.", response?.Errors);
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<BusDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusResponse>>($"api/buses/{id}", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load the bus.", response?.Errors);
        }

        return response.Data.Bus;
    }

    public async Task<BusDto> CreateAsync(CreateBusRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/buses", request, cancellationToken);
        ApiResponse<CreateBusResponse>? response = await ReadResponseAsync<CreateBusResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Bus;
    }

    public async Task UpdateAsync(Guid id, UpdateBusRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/buses/{id}", request, cancellationToken);
        ApiResponse<UpdateBusResponse>? response = await ReadResponseAsync<UpdateBusResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/buses/{id}", cancellationToken);
        ApiResponse<DeleteBusResponse>? response = await ReadResponseAsync<DeleteBusResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetReservationsByBusAsync(Guid busId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>($"api/bus-reservations/by-bus/{busId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load reservations.", response?.Errors);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetReservationsByEditionAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>($"api/bus-reservations/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load reservations.", response?.Errors);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetReservationsByRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>($"api/bus-reservations/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load reservations.", response?.Errors);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<BusReservationDto> CreateReservationAsync(CreateBusReservationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/bus-reservations", request, cancellationToken);
        ApiResponse<CreateBusReservationResponse>? response = await ReadResponseAsync<CreateBusReservationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Reservation;
    }

    public async Task<IReadOnlyList<BusReservationDto>> CreateReservationsAsync(CreateBusReservationsRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/bus-reservations/batch", request, cancellationToken);
        ApiResponse<GetBusReservationsResponse>? response = await ReadResponseAsync<GetBusReservationsResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Reservations;
    }

    public async Task<BusReservationDto> UpdateReservationAsync(Guid id, UpdateBusReservationRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/bus-reservations/{id}?isAdmin={isAdmin}", request, cancellationToken);
        ApiResponse<CreateBusReservationResponse>? response = await ReadResponseAsync<CreateBusReservationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Reservation;
    }

    public async Task DeleteReservationAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/bus-reservations/{id}?requestingRegistrationId={requestingRegistrationId}&isAdmin={isAdmin}", cancellationToken);
        ApiResponse<DeleteBusReservationResponse>? response = await ReadResponseAsync<DeleteBusReservationResponse>(httpResponse, cancellationToken);
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
}