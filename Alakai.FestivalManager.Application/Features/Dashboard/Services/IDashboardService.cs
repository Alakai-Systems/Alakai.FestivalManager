namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IDashboardService
{
    Task<ApiResponse<GetDashboardStatsResponse>> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RevenuePointDto>>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default);
}
