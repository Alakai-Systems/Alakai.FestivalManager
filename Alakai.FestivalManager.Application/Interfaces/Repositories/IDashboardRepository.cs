namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);

    /// <param name="range">"day" (last 15 calendar days, one point per day), "week" (last 8 calendar weeks, one point per week), "month" (last 6 calendar months, one point per month), "quarter" (last 4 natural calendar quarters, quarterly) or "year" (last 12 months, monthly). Ignored when <paramref name="customStart"/> and <paramref name="customEnd"/> are both provided.</param>
    /// <param name="offset">How many periods back from the most recent to anchor "day"/"week"/"month" on (0 = current period). Ignored for "quarter", "year" and the custom range.</param>
    /// <param name="customStart">Start of a free date range (inclusive). Only used together with <paramref name="customEnd"/>, and overrides <paramref name="range"/> when both are set.</param>
    /// <param name="customEnd">End of a free date range (inclusive). Only used together with <paramref name="customStart"/>, and overrides <paramref name="range"/> when both are set.</param>
    Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, int offset = 0, DateOnly? customStart = null, DateOnly? customEnd = null, CancellationToken cancellationToken = default);
}
