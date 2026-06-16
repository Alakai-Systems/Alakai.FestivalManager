namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;
public class CreateCompetitionCapacityCommand
{
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
