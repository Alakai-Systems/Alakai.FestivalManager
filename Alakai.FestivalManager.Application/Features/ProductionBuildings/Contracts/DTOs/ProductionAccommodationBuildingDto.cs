namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Contracts.DTOs;

public class ProductionAccommodationBuildingDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}