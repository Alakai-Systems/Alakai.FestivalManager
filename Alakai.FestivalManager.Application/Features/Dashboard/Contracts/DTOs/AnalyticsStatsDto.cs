namespace Alakai.FestivalManager.Application.Features.Dashboard.Contracts.DTOs;

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
