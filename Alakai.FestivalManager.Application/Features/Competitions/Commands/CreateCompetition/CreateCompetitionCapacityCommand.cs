namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionCapacityCommand
{
    /// <summary>Free-text level name this capacity belongs to (must match one of the competition's LevelNames). Null for competitions without levels.</summary>
    public string? LevelName { get; set; }

    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
