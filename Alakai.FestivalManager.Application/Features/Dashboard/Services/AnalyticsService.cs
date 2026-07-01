namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsClient _analyticsClient;
    private readonly IFestivalRepository _festivalRepository;

    public AnalyticsService(IAnalyticsClient analyticsClient, IFestivalRepository festivalRepository)
    {
        _analyticsClient = analyticsClient;
        _festivalRepository = festivalRepository;
    }

    public async Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(Guid festivalId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(festivalId, cancellationToken);

        if (festival is null || string.IsNullOrWhiteSpace(festival.GoogleAnalyticsPropertyId))
        {
            return new ApiResponse<AnalyticsStatsDto>
            {
                Success = false,
                Message = "Analytics unavailable",
                Data = new AnalyticsStatsDto
                {
                    IsAvailable = false,
                    ErrorMessage = festival is null
                        ? $"Festival with id '{festivalId}' was not found."
                        : $"Festival '{festival.Name}' does not have a Google Analytics Property ID configured. Set it in the Festivals table (GoogleAnalyticsPropertyId column)."
                },
                Errors = []
            };
        }

        AnalyticsStatsDto stats = await _analyticsClient.GetStatsAsync(festival.GoogleAnalyticsPropertyId, startDate, endDate, cancellationToken);

        return new ApiResponse<AnalyticsStatsDto>
        {
            Success = true,
            Message = stats.IsAvailable ? "Analytics retrieved successfully" : "Analytics unavailable",
            Data = stats,
            Errors = []
        };
    }
}
