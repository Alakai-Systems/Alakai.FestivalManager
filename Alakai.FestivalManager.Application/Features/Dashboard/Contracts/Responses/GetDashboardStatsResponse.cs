namespace Alakai.FestivalManager.Application.Features.Dashboard.Contracts.Responses;

public class GetDashboardStatsResponse
{
    public DashboardStatsDto Stats { get; set; } = new();
}
