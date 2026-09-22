namespace Alakai.FestivalManager.Application.Features.Editions.Commands.ResetEarlyBirdUsage;

public class ResetEarlyBirdUsageCommand
{
    public Guid EditionId { get; set; }

    public ResetEarlyBirdUsageCommand(Guid editionId)
    {
        EditionId = editionId;
    }
}