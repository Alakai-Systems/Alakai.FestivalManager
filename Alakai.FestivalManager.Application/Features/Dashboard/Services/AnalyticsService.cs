namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsClient _analyticsClient;

    public AnalyticsService(IAnalyticsClient analyticsClient)
    {
        _analyticsClient = analyticsClient;
    }

    public async Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        AnalyticsStatsDto stats = await _analyticsClient.GetStatsAsync(startDate, endDate, cancellationToken);

        return new ApiResponse<AnalyticsStatsDto>
        {
            Success = true,
            Message = stats.IsAvailable ? "Analytics retrieved successfully" : "Analytics unavailable",
            Data = stats,
            Errors = []
        };
    }
}
