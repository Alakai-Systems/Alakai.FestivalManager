namespace Alakai.FestivalManager.Application.Features.Editions.Contracts.Responses;

public class ResetEarlyBirdUsageResponse
{
    public Guid EditionId { get; set; }
    public int EarlyBirdUsedCount { get; set; }
}