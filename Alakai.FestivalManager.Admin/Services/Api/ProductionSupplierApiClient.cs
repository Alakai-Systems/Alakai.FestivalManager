using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ProductionSupplierApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ProductionSupplierApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<ProductionSupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionSuppliersResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionSuppliersResponse>>("api/ProductionSuppliers", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production suppliers.", response?.Errors);
        }

        return response.Data?.ProductionSuppliers ?? new List<ProductionSupplierDto>();
    }

    public async Task<IReadOnlyList<ProductionSupplierDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionSuppliersResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionSuppliersResponse>>($"api/ProductionSuppliers/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production suppliers for edition.", response?.Errors);
        }

        return response.Data?.ProductionSuppliers ?? new List<ProductionSupplierDto>();
    }

    public async Task CreateAsync(CreateProductionSupplierRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/ProductionSuppliers", request, cancellationToken);
        ApiResponse<CreateProductionSupplierResponse>? response = await ReadResponseAsync<CreateProductionSupplierResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductionSupplierRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/ProductionSuppliers/{id}", request, cancellationToken);
        ApiResponse<UpdateProductionSupplierResponse>? response = await ReadResponseAsync<UpdateProductionSupplierResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/ProductionSuppliers/{id}", cancellationToken);
        ApiResponse<DeleteProductionSupplierResponse>? response = await ReadResponseAsync<DeleteProductionSupplierResponse>(httpResponse, cancellationToken);

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