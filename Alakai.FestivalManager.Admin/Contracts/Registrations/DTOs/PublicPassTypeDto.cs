namespace Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;

public class PublicPassTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }
    public string? BuildingName { get; set; }
    public bool HasLevels => Levels.Count > 0;
    public List<PublicLevelDto> Levels { get; set; } = [];
}
