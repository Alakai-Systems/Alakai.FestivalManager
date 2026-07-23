namespace Alakai.FestivalManager.Admin.Services.Api;

public class ImpersonationApiClient
{
    private readonly HttpClient _httpClient;

    public ImpersonationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetTokenForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
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