using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class ImpersonationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ImpersonationApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
    {
        _httpClient = httpClient;
        _adminTokenProvider = adminTokenProvider;
    }

    public async Task<string?> GetTokenForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }

        HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/impersonation/by-registration/{registrationId}", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not start impersonation: {body}", null);
        }

        ImpersonationTokenResult? result = await response.Content.ReadFromJsonAsync<ImpersonationTokenResult>(cancellationToken: cancellationToken);
        return result?.AccessToken;
    }
}

public class ImpersonationTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
}