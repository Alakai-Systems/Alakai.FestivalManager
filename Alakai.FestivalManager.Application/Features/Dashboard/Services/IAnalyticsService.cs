namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IAnalyticsService
{
    Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(Guid festivalId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
