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
        if (string.IsNullOrWhiteSpace(_options.PropertyId) || string.IsNullOrWhiteSpace(_options.CredentialsPath))
        {
            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = "Google Analytics is not configured (missing PropertyId or CredentialsPath)."
            };
        }

        try
        {
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = _options.CredentialsPath
            }.Build();

            string property = $"properties/{_options.PropertyId}";
            DateRange range = new()
            {
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };

            RunReportRequest overviewRequest = new()
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

            RunReportResponse overviewResponse = await client.RunReportAsync(overviewRequest, cancellationToken);

            AnalyticsOverviewDto overview = new();

            if (overviewResponse.Rows.Count > 0)
            {
                Row row = overviewResponse.Rows[0];
                overview.TotalViews = ParseLong(row.MetricValues[0].Value);
                overview.ActiveUsers = ParseLong(row.MetricValues[1].Value);
                overview.EventCount = ParseLong(row.MetricValues[2].Value);
                overview.NewUsers = ParseLong(row.MetricValues[3].Value);
            }

            RunReportRequest countriesRequest = new()
            {
                Property = property,
                DateRanges = { range },
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
                DateRanges = { range },
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
                ErrorMessage = "Could not retrieve Google Analytics data. Check server logs for details."
            };
        }
    }

    private static long ParseLong(string value) => long.TryParse(value, out long result) ? result : 0;
}
