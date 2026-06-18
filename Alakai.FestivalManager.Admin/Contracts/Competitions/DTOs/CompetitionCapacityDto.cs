namespace Alakai.FestivalManager.Admin.Contracts.Competitions.DTOs;

public class CompetitionCapacityDto
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
