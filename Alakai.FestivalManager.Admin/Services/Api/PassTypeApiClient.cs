namespace Alakai.FestivalManager.Admin.Services.Api;

public class PassTypeApiClient
{
    private readonly HttpClient _httpClient;

    public PassTypeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PassTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Controller is 'PassTypesController' -> route token is "PassTypes" (no hyphen).
        ApiResponse<GetPassTypesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypesResponse>>("api/PassTypes", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass types.", response?.Errors);
        }

        return response.Data?.PassTypes ?? new List<PassTypeDto>();
    }

    public async Task<IReadOnlyList<PassTypeDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetPassTypesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypesResponse>>($"api/PassTypes/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass types for edition.", response?.Errors);
        }

        return response.Data?.PassTypes ?? new List<PassTypeDto>();
    }

    public async Task<PassTypeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetPassTypeByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypeByIdResponse>>($"api/PassTypes/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass type.", response?.Errors);
        }

        return response.Data!.PassType;
    }

    public async Task CreateAsync(CreatePassTypeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/PassTypes", request, cancellationToken);
        ApiResponse<CreatePassTypeResponse>? response = await ReadResponseAsync<CreatePassTypeResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdatePassTypeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/PassTypes/{id}", request, cancellationToken);
        ApiResponse<UpdatePassTypeResponse>? response = await ReadResponseAsync<UpdatePassTypeResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/PassTypes/{id}", cancellationToken);
        ApiResponse<DeletePassTypeResponse>? response = await ReadResponseAsync<DeletePassTypeResponse>(httpResponse, cancellationToken);

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