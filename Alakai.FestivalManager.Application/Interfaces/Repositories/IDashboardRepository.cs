namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);

    /// <param name="range">"week" (last 7 days, daily), "month" (last 30 days, daily) or "year" (last 12 months, monthly).</param>
    Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default);
}
