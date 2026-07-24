using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ProductionTripApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ProductionTripApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
    {
        _httpClient = httpClient;
        _adminTokenProvider = adminTokenProvider;
    }

    private async Task AttachAuthHeaderAsync()
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }
    }

    public async Task<IReadOnlyList<ProductionTripDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetTripsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetTripsResponse>>($"api/ProductionTrips/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load trips.", response?.Errors);
        }

        return response.Data?.Trips ?? new List<ProductionTripDto>();
    }

    public async Task CreateAsync(CreateTripRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/ProductionTrips", request, cancellationToken);
        ApiResponse<CreateTripResponse>? response = await ReadResponseAsync<CreateTripResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateTripRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/ProductionTrips/{id}", request, cancellationToken);
        ApiResponse<UpdateTripResponse>? response = await ReadResponseAsync<UpdateTripResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/ProductionTrips/{id}", cancellationToken);
        ApiResponse<DeleteTripResponse>? response = await ReadResponseAsync<DeleteTripResponse>(httpResponse, cancellationToken);

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
        IReadOnlyList<string> errors = response?.Errors ?? new List<string>();

        throw new ApiClientException(message, errors);
    }
}