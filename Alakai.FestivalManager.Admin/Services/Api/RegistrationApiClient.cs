using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
using Alakai.FestivalManager.Admin.Contracts.Registrations.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class RegistrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public RegistrationApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<RegistrationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetRegistrationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationsResponse>>("api/registrations", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        return response.Data?.Registrations ?? [];
    }

    public async Task<IReadOnlyList<RegistrationDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetRegistrationsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationsByEditionIdResponse>>($"api/registrations/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        return response.Data?.Registrations ?? [];
    }

    public async Task<string> GetByEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetRegistrationsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationsByEditionIdResponse>>($"api/registrations/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        string firstName = response.Data?.Registrations.FirstOrDefault(c => c.Email == email)?.FirstName ?? string.Empty;
        string lastName = response.Data?.Registrations.FirstOrDefault(c => c.Email == email).LastName ?? string.Empty;
        string name = $"{firstName} {lastName}".Trim();

        return name;
    }

    public async Task<RegistrationDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetRegistrationByUserIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetRegistrationByUserIdResponse>>($"api/registrations/by-userId/{userId}", cancellationToken);

        if (response is null || response.Data is null || response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load registrations.", response?.Errors);
        }

        return response.Data.Registration;
    }

    public async Task UpdateAsync(Guid id, UpdateRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/registrations/{id}", request, cancellationToken);
        ApiResponse<UpdateRegistrationResponse>? response = await ReadResponseAsync<UpdateRegistrationResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

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
