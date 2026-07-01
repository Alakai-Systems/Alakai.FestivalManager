namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IAnalyticsClient
{
    Task<AnalyticsStatsDto> GetStatsAsync(string propertyId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
