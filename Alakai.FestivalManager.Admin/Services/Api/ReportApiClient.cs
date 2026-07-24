using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class ReportApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public ReportApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<byte[]> GetReportXlsxAsync(string reportType, Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage response = await _httpClient.GetAsync($"api/reports/{reportType}?editionId={editionId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException(string.IsNullOrWhiteSpace(errorContent) ? $"Request failed with status code {(int)response.StatusCode}." : errorContent);
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}