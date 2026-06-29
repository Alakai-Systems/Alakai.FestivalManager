namespace Alakai.FestivalManager.Admin.Services.Api;

public class AnalyticsOverviewDto
{
    public long TotalViews { get; set; }
    public long ActiveUsers { get; set; }
    public long EventCount { get; set; }
    public long NewUsers { get; set; }
}

public class AnalyticsCountryStatDto
{
    public string Country { get; set; } = string.Empty;
    public long ActiveUsers { get; set; }
}

public class AnalyticsPageStatDto
{
    public string PagePath { get; set; } = string.Empty;
    public long Views { get; set; }
}

public class AnalyticsStatsDto
{
    public bool IsAvailable { get; set; }
    public string? ErrorMessage { get; set; }
    public AnalyticsOverviewDto Overview { get; set; } = new();
    public List<AnalyticsCountryStatDto> TopCountries { get; set; } = [];
    public List<AnalyticsPageStatDto> TopPages { get; set; } = [];
}

internal class AnalyticsApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AnalyticsStatsDto? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class AnalyticsApiClient
{
    private readonly HttpClient _httpClient;

    public AnalyticsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AnalyticsStatsDto> GetAnalyticsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        string start = startDate.ToString("yyyy-MM-dd");
        string end = endDate.ToString("yyyy-MM-dd");

        AnalyticsApiResponse? response = await _httpClient.GetFromJsonAsync<AnalyticsApiResponse>(
            $"api/dashboard/analytics?startDate={start}&endDate={end}", cancellationToken);

        return response?.Data ?? new AnalyticsStatsDto { IsAvailable = false, ErrorMessage = "Empty response." };
    }
}
