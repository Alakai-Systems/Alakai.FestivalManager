namespace Alakai.FestivalManager.Admin.Contracts.Competitions.Requests;

public class UpdateCompetitionCapacityRequest
{
    public string? LevelName { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
