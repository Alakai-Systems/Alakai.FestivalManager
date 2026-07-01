namespace Alakai.FestivalManager.Domain.Entities;

public class CompetitionCapacity : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public Guid? CompetitionLevelId { get; set; }
    public CompetitionLevel? CompetitionLevel { get; set; }

    public DanceRole DanceRole { get; set; }

    public int Capacity { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
