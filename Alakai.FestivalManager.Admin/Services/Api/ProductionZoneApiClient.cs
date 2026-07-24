using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ProductionZoneApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ProductionZoneApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<ProductionAccommodationZoneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionAccommodationZonesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionAccommodationZonesResponse>>("api/ProductionAccommodationZones", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production accommodation zones.", response?.Errors);
        }

        return response.Data?.ProductionAccommodationZones ?? new List<ProductionAccommodationZoneDto>();
    }

    public async Task<IReadOnlyList<ProductionAccommodationZoneDto>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionAccommodationZonesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionAccommodationZonesResponse>>($"api/ProductionAccommodationZones/by-building/{buildingId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production accommodation zones.", response?.Errors);
        }

        return response.Data?.ProductionAccommodationZones ?? new List<ProductionAccommodationZoneDto>();
    }

    public async Task CreateAsync(CreateProductionAccommodationZoneRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/ProductionAccommodationZones", request, cancellationToken);
        ApiResponse<CreateProductionAccommodationZoneResponse>? response = await ReadResponseAsync<CreateProductionAccommodationZoneResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductionAccommodationZoneRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/ProductionAccommodationZones/{id}", request, cancellationToken);
        ApiResponse<UpdateProductionAccommodationZoneResponse>? response = await ReadResponseAsync<UpdateProductionAccommodationZoneResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/ProductionAccommodationZones/{id}", cancellationToken);
        ApiResponse<DeleteProductionAccommodationZoneResponse>? response = await ReadResponseAsync<DeleteProductionAccommodationZoneResponse>(httpResponse, cancellationToken);

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