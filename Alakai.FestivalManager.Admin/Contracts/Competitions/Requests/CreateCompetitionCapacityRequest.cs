namespace Alakai.FestivalManager.Admin.Contracts.Competitions.Requests;

public class CreateCompetitionCapacityRequest
{
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
