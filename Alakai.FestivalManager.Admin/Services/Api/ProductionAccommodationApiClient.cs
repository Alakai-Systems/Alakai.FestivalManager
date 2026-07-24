using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ProductionAccommodationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ProductionAccommodationApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<ProductionAccommodationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionAccommodationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionAccommodationsResponse>>("api/ProductionAccommodations", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production accommodations.", response?.Errors);
        }

        return response.Data?.ProductionAccommodations ?? new List<ProductionAccommodationDto>();
    }

    public async Task<IReadOnlyList<ProductionAccommodationDto>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionAccommodationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionAccommodationsResponse>>($"api/ProductionAccommodations/by-zone/{zoneId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production accommodations for zone.", response?.Errors);
        }

        return response.Data?.ProductionAccommodations ?? new List<ProductionAccommodationDto>();
    }

    public async Task CreateAsync(CreateProductionAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/ProductionAccommodations", request, cancellationToken);
        ApiResponse<CreateProductionAccommodationResponse>? response = await ReadResponseAsync<CreateProductionAccommodationResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductionAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/ProductionAccommodations/{id}", request, cancellationToken);
        ApiResponse<UpdateProductionAccommodationResponse>? response = await ReadResponseAsync<UpdateProductionAccommodationResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/ProductionAccommodations/{id}", cancellationToken);
        ApiResponse<DeleteProductionAccommodationResponse>? response = await ReadResponseAsync<DeleteProductionAccommodationResponse>(httpResponse, cancellationToken);

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