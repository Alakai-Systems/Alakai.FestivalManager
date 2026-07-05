namespace Alakai.FestivalManager.Admin.Services.Api;

public class ReportApiClient
{
    private readonly HttpClient _httpClient;

    public ReportApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetReportXlsxAsync(string reportType, Guid editionId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/reports/{reportType}?editionId={editionId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException(string.IsNullOrWhiteSpace(errorContent) ? $"Request failed with status code {(int)response.StatusCode}." : errorContent);
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}