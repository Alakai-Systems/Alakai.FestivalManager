namespace Alakai.FestivalManager.Application.Features.Levels.Commands.DeleteLevel;

public class DeleteLevelCommand
{
    public Guid Id { get; set; }
    public DeleteLevelCommand(Guid id)
    {
        Id = id;
    }
}
