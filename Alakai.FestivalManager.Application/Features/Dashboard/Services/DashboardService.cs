namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<ApiResponse<GetDashboardStatsResponse>> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        DashboardStatsDto stats = await _dashboardRepository.GetStatsAsync(editionId, cancellationToken);

        return new ApiResponse<GetDashboardStatsResponse>
        {
            Success = true,
            Message = "Dashboard stats retrieved successfully",
            Data = new GetDashboardStatsResponse { Stats = stats },
            Errors = []
        };
    }

    public async Task<ApiResponse<List<RevenuePointDto>>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default)
    {
        List<RevenuePointDto> points = await _dashboardRepository.GetRevenueAsync(editionId, range, cancellationToken);

        return new ApiResponse<List<RevenuePointDto>>
        {
            Success = true,
            Message = "Revenue retrieved successfully",
            Data = points,
            Errors = []
        };
    }
}
