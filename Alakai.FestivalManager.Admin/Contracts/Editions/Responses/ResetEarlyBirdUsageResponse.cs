using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class ResetEarlyBirdUsageResponse
{
    public Guid EditionId { get; set; }

    public int EarlyBirdUsedCount { get; set; }
}