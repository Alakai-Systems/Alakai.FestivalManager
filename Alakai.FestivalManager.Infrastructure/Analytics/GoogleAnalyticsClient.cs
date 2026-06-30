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

    public async Task<AnalyticsStatsDto> GetStatsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PropertyId))
        {
            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = "GoogleAnalytics:PropertyId is empty or missing from configuration."
            };
        }

        if (string.IsNullOrWhiteSpace(_options.CredentialsPath))
        {
            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = "GoogleAnalytics:CredentialsPath is empty or missing from configuration."
            };
        }

        if (!File.Exists(_options.CredentialsPath))
        {
            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = $"Credentials file not found at: {_options.CredentialsPath}"
            };
        }

        try
        {
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = _options.CredentialsPath
            }.Build();

            string property = $"properties/{_options.PropertyId}";

            int spanDays = (endDate.DayNumber - startDate.DayNumber) + 1;
            DateOnly previousEnd = startDate.AddDays(-1);
            DateOnly previousStart = previousEnd.AddDays(-(spanDays - 1));

            DateRange currentRange = new() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") };
            DateRange previousRange = new() { StartDate = previousStart.ToString("yyyy-MM-dd"), EndDate = previousEnd.ToString("yyyy-MM-dd") };

            (long views, long users, long events, long newUsers) current = await GetOverviewNumbersAsync(client, property, currentRange, cancellationToken);
            (long views, long users, long events, long newUsers) previous = await GetOverviewNumbersAsync(client, property, previousRange, cancellationToken);

            (List<long> views, List<long> users, List<long> events, List<long> newUsers) daily =
                await GetDailySeriesAsync(client, property, currentRange, cancellationToken);

            AnalyticsOverviewDto overview = new()
            {
                TotalViews = current.views,
                ActiveUsers = current.users,
                EventCount = current.events,
                NewUsers = current.newUsers,
                TotalViewsChangePercent = PercentChange(current.views, previous.views),
                ActiveUsersChangePercent = PercentChange(current.users, previous.users),
                EventCountChangePercent = PercentChange(current.events, previous.events),
                NewUsersChangePercent = PercentChange(current.newUsers, previous.newUsers),
                ViewsSparkline = daily.views,
                ActiveUsersSparkline = daily.users,
                EventCountSparkline = daily.events,
                NewUsersSparkline = daily.newUsers
            };

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
                .Select(r => new AnalyticsCountryStatDto
                {
                    Country = r.DimensionValues[0].Value,
                    ActiveUsers = ParseLong(r.MetricValues[0].Value)
                })
                .ToList();

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
                .Select(r => new AnalyticsPageStatDto
                {
                    PagePath = r.DimensionValues[0].Value,
                    Views = ParseLong(r.MetricValues[0].Value)
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
