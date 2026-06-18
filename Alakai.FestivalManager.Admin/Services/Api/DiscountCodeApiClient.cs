namespace Alakai.FestivalManager.Admin.Services.Api;

public class DiscountCodeApiClient
{
    private readonly HttpClient _httpClient;

    public DiscountCodeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetDiscountCodesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetDiscountCodesResponse>>("api/discount-codes", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load discount codes.", response?.Errors);
        }

        return response.Data?.DiscountCodes ?? [];
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetDiscountCodesByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetDiscountCodesByEditionIdResponse>>($"api/discount-codes/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load discount codes.", response?.Errors);
        }

        return response.Data?.DiscountCodes ?? [];
    }

    public async Task CreateAsync(CreateDiscountCodeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/discount-codes", request, cancellationToken);
        ApiResponse<CreateDiscountCodeResponse>? response = await ReadResponseAsync<CreateDiscountCodeResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateDiscountCodeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/discount-codes/{id}", request, cancellationToken);
        ApiResponse<UpdateDiscountCodeResponse>? response = await ReadResponseAsync<UpdateDiscountCodeResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/discount-codes/{id}", cancellationToken);
        ApiResponse<DeleteDiscountCodeResponse>? response = await ReadResponseAsync<DeleteDiscountCodeResponse>(httpResponse, cancellationToken);
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
