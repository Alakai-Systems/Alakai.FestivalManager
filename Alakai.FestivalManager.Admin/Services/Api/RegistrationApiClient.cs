namespace Alakai.FestivalManager.Admin.Services.Api;

public class RegistrationApiClient
{
    private readonly HttpClient _httpClient;

    public RegistrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RegistrationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetRegistrationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationsResponse>>("api/registrations", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        return response.Data?.Registrations ?? [];
    }

    public async Task<IReadOnlyList<RegistrationDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetRegistrationsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationsByEditionIdResponse>>($"api/registrations/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        return response.Data?.Registrations ?? [];
    }

    public async Task UpdateAsync(Guid id, UpdateRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/registrations/{id}", request, cancellationToken);
        ApiResponse<UpdateRegistrationResponse>? response = await ReadResponseAsync<UpdateRegistrationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/registrations/{id}", cancellationToken);
        ApiResponse<DeleteRegistrationResponse>? response = await ReadResponseAsync<DeleteRegistrationResponse>(httpResponse, cancellationToken);
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
