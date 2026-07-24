using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ProductionPersonApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ProductionPersonApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<ProductionPersonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionPeopleResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionPeopleResponse>>("api/ProductionPeople", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production people.", response?.Errors);
        }

        return response.Data?.ProductionPeople ?? new List<ProductionPersonDto>();
    }

    public async Task<IReadOnlyList<ProductionPersonDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionPeopleResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionPeopleResponse>>($"api/ProductionPeople/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production people for edition.", response?.Errors);
        }

        return response.Data?.ProductionPeople ?? new List<ProductionPersonDto>();
    }

    public async Task<ProductionPersonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetProductionPersonByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetProductionPersonByIdResponse>>($"api/ProductionPeople/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load production person.", response?.Errors);
        }

        return response.Data!.ProductionPerson;
    }

    public async Task CreateAsync(CreateProductionPersonRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/ProductionPeople", request, cancellationToken);
        ApiResponse<CreateProductionPersonResponse>? response = await ReadResponseAsync<CreateProductionPersonResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductionPersonRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/ProductionPeople/{id}", request, cancellationToken);
        ApiResponse<UpdateProductionPersonResponse>? response = await ReadResponseAsync<UpdateProductionPersonResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/ProductionPeople/{id}", cancellationToken);
        ApiResponse<DeleteProductionPersonResponse>? response = await ReadResponseAsync<DeleteProductionPersonResponse>(httpResponse, cancellationToken);

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