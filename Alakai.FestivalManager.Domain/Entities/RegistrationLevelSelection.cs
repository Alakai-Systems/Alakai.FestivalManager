namespace Alakai.FestivalManager.Domain.Entities;

/// <summary>
/// Stores the individual levels selected in a single registration when the pass type
/// allows selecting multiple levels (e.g. "single parties" passes where each party is
/// modeled as a Level). The registration's primary LevelId points to the first
/// selected level for backwards compatibility; ALL selected levels (including the
/// primary one) are recorded here so per-level capacity counts stay accurate.
/// </summary>
public class RegistrationLevelSelection : BaseEntity
{
    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public Guid LevelId { get; set; }
    public Level Level { get; set; } = default!;
}
