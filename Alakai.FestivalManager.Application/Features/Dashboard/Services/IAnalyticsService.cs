namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IAnalyticsService
{
    Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
