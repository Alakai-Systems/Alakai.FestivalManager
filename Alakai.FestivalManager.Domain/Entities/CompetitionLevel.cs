namespace Alakai.FestivalManager.Domain.Entities;

/// <summary>
/// A configurable level/category within a single Competition (e.g. "Open", "Advanced",
/// "Beginner"). Replaces the old fixed MixAndMatchLevel enum (which only allowed 2 values),
/// so an admin can define as many levels as a given competition actually needs, or none at all
/// for competitions that don't use levels (e.g. a simple Individual-format competition).
/// </summary>
public class CompetitionLevel : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
