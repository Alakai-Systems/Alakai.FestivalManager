namespace Alakai.FestivalManager.Application.Features.Dashboard.Contracts.DTOs;

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