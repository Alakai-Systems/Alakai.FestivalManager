namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

public class PublicPassTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }

    /// <summary>Set when the pass is linked to exactly one accommodation building (used to group passes in the form).</summary>
    public string? BuildingName { get; set; }
    public bool HasLevels => Levels.Count > 0;
    public List<PublicLevelDto> Levels { get; set; } = [];
}
