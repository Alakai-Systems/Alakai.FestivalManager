namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IDashboardService
{
    Task<ApiResponse<GetDashboardStatsResponse>> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RevenuePointDto>>> GetRevenueAsync(Guid editionId, string range, int offset = 0, DateOnly? customStart = null, DateOnly? customEnd = null, CancellationToken cancellationToken = default);
}
