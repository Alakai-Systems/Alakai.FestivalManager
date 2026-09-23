namespace Alakai.FestivalManager.Application.Features.Editions.Commands.RemoveEditionSchedule;

public class RemoveEditionScheduleCommand
{
    public Guid EditionId { get; set; }

    public RemoveEditionScheduleCommand(Guid editionId)
    {
        EditionId = editionId;
    }
}