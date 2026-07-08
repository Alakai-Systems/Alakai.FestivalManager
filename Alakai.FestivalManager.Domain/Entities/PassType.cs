namespace Alakai.FestivalManager.Domain.Entities;

public class PassType : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }
    public ICollection<Level> Levels { get; set; } = new List<Level>();
}
