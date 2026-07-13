using Google.Analytics.Data.V1Beta;
using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Infrastructure.Analytics;

public class GoogleAnalyticsClient : IAnalyticsClient
{
    private readonly GoogleAnalyticsOptions _options;
    private readonly ILogger<GoogleAnalyticsClient> _logger;

    public GoogleAnalyticsClient(IOptions<GoogleAnalyticsOptions> options, ILogger<GoogleAnalyticsClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AnalyticsStatsDto> GetStatsAsync(string propertyId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        bool hasFile = !string.IsNullOrWhiteSpace(_options.CredentialsPath) && File.Exists(_options.CredentialsPath);
        bool hasJson = !string.IsNullOrWhiteSpace(_options.CredentialsJson);
        if (!hasFile && !hasJson)
        {
            return new AnalyticsStatsDto { IsAvailable = false, ErrorMessage = "Google Analytics credentials not configured." };
        }

        try
        {
            BetaAnalyticsDataClientBuilder builder = hasFile
                ? new BetaAnalyticsDataClientBuilder { CredentialsPath = _options.CredentialsPath }
                : new BetaAnalyticsDataClientBuilder { JsonCredentials = _options.CredentialsJson };
            BetaAnalyticsDataClient client = builder.Build();

            string property = $"properties/{propertyId}";

            DateRange currentRange = new() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") };

            // Week-over-week comparison window, used ONLY for the change-percent badges
            // (KPI cards, Top Countries, Top Pages). This is intentionally independent of
            // the overall selected date range (which still drives the totals shown, the
            // sparklines, and the Top Countries/Pages ranking) -- the comparison itself is
            // always "last 7 days vs the 7 days before that", anchored on the range's end date.
            DateOnly thisWeekEnd = endDate;
            DateOnly thisWeekStart = thisWeekEnd.AddDays(-6);
            DateOnly previousWeekEnd = thisWeekStart.AddDays(-1);
            DateOnly previousWeekStart = previousWeekEnd.AddDays(-6);

            DateRange thisWeekRange = new() { StartDate = thisWeekStart.ToString("yyyy-MM-dd"), EndDate = thisWeekEnd.ToString("yyyy-MM-dd") };
            DateRange previousWeekRange = new() { StartDate = previousWeekStart.ToString("yyyy-MM-dd"), EndDate = previousWeekEnd.ToString("yyyy-MM-dd") };

            (long views, long users, long events, long newUsers) current = await GetOverviewNumbersAsync(client, property, currentRange, cancellationToken);
            (long views, long users, long events, long newUsers) thisWeek = await GetOverviewNumbersAsync(client, property, thisWeekRange, cancellationToken);
            (long views, long users, long events, long newUsers) previousWeek = await GetOverviewNumbersAsync(client, property, previousWeekRange, cancellationToken);

            (List<long> views, List<long> users, List<long> events, List<long> newUsers) daily =
                await GetDailySeriesAsync(client, property, currentRange, cancellationToken);

            AnalyticsOverviewDto overview = new()
            {
                TotalViews = current.views,
                ActiveUsers = current.users,
                EventCount = current.events,
                NewUsers = current.newUsers,
                TotalViewsChangePercent = PercentChange(thisWeek.views, previousWeek.views),
                ActiveUsersChangePercent = PercentChange(thisWeek.users, previousWeek.users),
                EventCountChangePercent = PercentChange(thisWeek.events, previousWeek.events),
                NewUsersChangePercent = PercentChange(thisWeek.newUsers, previousWeek.newUsers),
                ViewsSparkline = daily.views,
                ActiveUsersSparkline = daily.users,
                EventCountSparkline = daily.events,
                NewUsersSparkline = daily.newUsers
            };

            // Top Countries / Top Pages: the ranking and the displayed totals still come
            // from the overall selected range (unchanged). The change-percent per row
            // compares that same country/page's activeUsers/views this week vs the
            // previous week, matched by dimension value.
            Dictionary<string, long> countriesThisWeek = await GetCountryBreakdownAsync(client, property, thisWeekRange, cancellationToken);
            Dictionary<string, long> countriesPreviousWeek = await GetCountryBreakdownAsync(client, property, previousWeekRange, cancellationToken);

            RunReportRequest countriesRequest = new()
            {
                Property = property,
                DateRanges = { currentRange },
                Dimensions = { new Dimension { Name = "country" } },
                Metrics = { new Metric { Name = "activeUsers" } },
                OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "activeUsers" }, Desc = true } },
                Limit = 5
            };

            RunReportResponse countriesResponse = await client.RunReportAsync(countriesRequest, cancellationToken);

            List<AnalyticsCountryStatDto> topCountries = countriesResponse.Rows
                .Select(r =>
                {
                    string country = r.DimensionValues[0].Value;
                    countriesThisWeek.TryGetValue(country, out long thisWeekValue);
                    countriesPreviousWeek.TryGetValue(country, out long previousWeekValue);

                    return new AnalyticsCountryStatDto
                    {
                        Country = country,
                        ActiveUsers = ParseLong(r.MetricValues[0].Value),
                        ActiveUsersChangePercent = PercentChange(thisWeekValue, previousWeekValue)
                    };
                })
                .ToList();

            Dictionary<string, long> pagesThisWeek = await GetPageBreakdownAsync(client, property, thisWeekRange, cancellationToken);
            Dictionary<string, long> pagesPreviousWeek = await GetPageBreakdownAsync(client, property, previousWeekRange, cancellationToken);

            RunReportRequest pagesRequest = new()
            {
                Property = property,
                DateRanges = { currentRange },
                Dimensions = { new Dimension { Name = "pagePath" } },
                Metrics = { new Metric { Name = "screenPageViews" } },
                OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "screenPageViews" }, Desc = true } },
                Limit = 5
            };

            RunReportResponse pagesResponse = await client.RunReportAsync(pagesRequest, cancellationToken);

            List<AnalyticsPageStatDto> topPages = pagesResponse.Rows
                .Select(r =>
                {
                    string pagePath = r.DimensionValues[0].Value;
                    pagesThisWeek.TryGetValue(pagePath, out long thisWeekValue);
                    pagesPreviousWeek.TryGetValue(pagePath, out long previousWeekValue);

                    return new AnalyticsPageStatDto
                    {
                        PagePath = pagePath,
                        Views = ParseLong(r.MetricValues[0].Value),
                        ViewsChangePercent = PercentChange(thisWeekValue, previousWeekValue)
                    };
                })
                .ToList();

            return new AnalyticsStatsDto
            {
                IsAvailable = true,
                DateRangeLabel = $"{startDate:dd MMM} - {endDate:dd MMM yyyy}",
                Overview = overview,
                TopCountries = topCountries,
                TopPages = topPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Google Analytics data.");

            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = $"{ex.GetType().Name}: {ex.Message}"
            };
        }
    }

    private static async Task<(long views, long users, long events, long newUsers)> GetOverviewNumbersAsync(
        BetaAnalyticsDataClient client, string property, DateRange range, CancellationToken cancellationToken)
    {
        RunReportRequest request = new()
        {
            Property = property,
            DateRanges = { range },
            Metrics =
            {
                new Metric { Name = "screenPageViews" },
                new Metric { Name = "activeUsers" },
                new Metric { Name = "eventCount" },
                new Metric { Name = "newUsers" }
            }
        };

        RunReportResponse response = await client.RunReportAsync(request, cancellationToken);

        if (response.Rows.Count == 0)
        {
            return (0, 0, 0, 0);
        }

        Row row = response.Rows[0];

        return (
            ParseLong(row.MetricValues[0].Value),
            ParseLong(row.MetricValues[1].Value),
            ParseLong(row.MetricValues[2].Value),
            ParseLong(row.MetricValues[3].Value)
        );
    }

    private static async Task<Dictionary<string, long>> GetCountryBreakdownAsync(
        BetaAnalyticsDataClient client, string property, DateRange range, CancellationToken cancellationToken)
    {
        RunReportRequest request = new()
        {
            Property = property,
            DateRanges = { range },
            Dimensions = { new Dimension { Name = "country" } },
            Metrics = { new Metric { Name = "activeUsers" } }
        };

        RunReportResponse response = await client.RunReportAsync(request, cancellationToken);

        return response.Rows.ToDictionary(r => r.DimensionValues[0].Value, r => ParseLong(r.MetricValues[0].Value));
    }

    private static async Task<Dictionary<string, long>> GetPageBreakdownAsync(
        BetaAnalyticsDataClient client, string property, DateRange range, CancellationToken cancellationToken)
    {
        RunReportRequest request = new()
        {
            Property = property,
            DateRanges = { range },
            Dimensions = { new Dimension { Name = "pagePath" } },
            Metrics = { new Metric { Name = "screenPageViews" } }
        };

        RunReportResponse response = await client.RunReportAsync(request, cancellationToken);

        return response.Rows.ToDictionary(r => r.DimensionValues[0].Value, r => ParseLong(r.MetricValues[0].Value));
    }

    private static async Task<(List<long> views, List<long> users, List<long> events, List<long> newUsers)> GetDailySeriesAsync(
        BetaAnalyticsDataClient client, string property, DateRange range, CancellationToken cancellationToken)
    {
        RunReportRequest request = new()
        {
            Property = property,
            DateRanges = { range },
            Dimensions = { new Dimension { Name = "date" } },
            Metrics =
            {
                new Metric { Name = "screenPageViews" },
                new Metric { Name = "activeUsers" },
                new Metric { Name = "eventCount" },
                new Metric { Name = "newUsers" }
            },
            OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "date" } } }
        };

        RunReportResponse response = await client.RunReportAsync(request, cancellationToken);

        List<long> views = [];
        List<long> users = [];
        List<long> events = [];
        List<long> newUsers = [];

        foreach (Row row in response.Rows)
        {
            views.Add(ParseLong(row.MetricValues[0].Value));
            users.Add(ParseLong(row.MetricValues[1].Value));
            events.Add(ParseLong(row.MetricValues[2].Value));
            newUsers.Add(ParseLong(row.MetricValues[3].Value));
        }

        return (views, users, events, newUsers);
    }

    private static decimal? PercentChange(long current, long previous)
    {
        if (previous == 0)
        {
            return null;
        }

        return Math.Round(((decimal)(current - previous) / previous) * 100, 1);
    }

    private static long ParseLong(string value) => long.TryParse(value, out long result) ? result : 0;
}