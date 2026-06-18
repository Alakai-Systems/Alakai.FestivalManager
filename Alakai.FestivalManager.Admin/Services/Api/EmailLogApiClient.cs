using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailLogApiClient
{
    private readonly HttpClient _httpClient;

    public EmailLogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLogsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsResponse>>("api/email-logs", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLogsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByEditionIdResponse>>($"api/email-logs/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLogsByRegistrationIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByRegistrationIdResponse>>($"api/email-logs/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLogsByUserIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByUserIdResponse>>($"api/email-logs/by-user/{userId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task CreateAsync(CreateEmailLogRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/email-logs", request, cancellationToken);
        ApiResponse<CreateEmailLogResponse>? response = await ReadResponseAsync<CreateEmailLogResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEmailLogRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/email-logs/{id}", request, cancellationToken);
        ApiResponse<UpdateEmailLogResponse>? response = await ReadResponseAsync<UpdateEmailLogResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/email-logs/{id}", cancellationToken);
        ApiResponse<DeleteEmailLogResponse>? response = await ReadResponseAsync<DeleteEmailLogResponse>(httpResponse, cancellationToken);

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
