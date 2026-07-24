using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailLogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public EmailLogApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<EmailLogDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailLogsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsResponse>>("api/email-logs", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailLogsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByEditionIdResponse>>($"api/email-logs/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailLogsByRegistrationIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByRegistrationIdResponse>>($"api/email-logs/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailLogsByUserIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLogsByUserIdResponse>>($"api/email-logs/by-user/{userId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email logs.", response?.Errors);
        }

        return response.Data?.EmailLogs ?? [];
    }

    public async Task CreateAsync(CreateEmailLogRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/email-logs", request, cancellationToken);
        ApiResponse<CreateEmailLogResponse>? response = await ReadResponseAsync<CreateEmailLogResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEmailLogRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/email-logs/{id}", request, cancellationToken);
        ApiResponse<UpdateEmailLogResponse>? response = await ReadResponseAsync<UpdateEmailLogResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

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
