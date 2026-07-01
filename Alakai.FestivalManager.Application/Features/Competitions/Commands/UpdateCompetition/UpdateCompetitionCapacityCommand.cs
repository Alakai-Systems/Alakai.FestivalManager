namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionCapacityCommand
{
    public Guid? Id { get; set; }
    public string? LevelName { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
