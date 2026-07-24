using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class AnalyticsOverviewDto
{
    public long TotalViews { get; set; }
    public long ActiveUsers { get; set; }
    public long EventCount { get; set; }
    public long NewUsers { get; set; }
    public decimal? TotalViewsChangePercent { get; set; }
    public decimal? ActiveUsersChangePercent { get; set; }
    public decimal? EventCountChangePercent { get; set; }
    public decimal? NewUsersChangePercent { get; set; }
    public List<long> ViewsSparkline { get; set; } = [];
    public List<long> ActiveUsersSparkline { get; set; } = [];
    public List<long> EventCountSparkline { get; set; } = [];
    public List<long> NewUsersSparkline { get; set; } = [];
}

public class AnalyticsCountryStatDto
{
    public string Country { get; set; } = string.Empty;
    public long ActiveUsers { get; set; }
    public decimal? ActiveUsersChangePercent { get; set; }
}

public class AnalyticsPageStatDto
{
    public string PagePath { get; set; } = string.Empty;
    public long Views { get; set; }
    public decimal? ViewsChangePercent { get; set; }
}

public class AnalyticsStatsDto
{
    public bool IsAvailable { get; set; }
    public string? ErrorMessage { get; set; }
    public string DateRangeLabel { get; set; } = string.Empty;
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
    private readonly IAdminTokenProvider _adminTokenProvider;

    public AnalyticsApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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
    public async Task<AnalyticsStatsDto> GetAnalyticsAsync(Guid festivalId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        string start = startDate.ToString("yyyy-MM-dd");
        string end = endDate.ToString("yyyy-MM-dd");
        AnalyticsApiResponse? response = await _httpClient.GetFromJsonAsync<AnalyticsApiResponse>(
            $"api/dashboard/analytics?festivalId={festivalId}&startDate={start}&endDate={end}", cancellationToken);
        return response?.Data ?? new AnalyticsStatsDto { IsAvailable = false, ErrorMessage = "Empty response." };
    }
}