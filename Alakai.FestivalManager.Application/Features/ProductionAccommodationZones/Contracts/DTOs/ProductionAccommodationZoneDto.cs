namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Contracts.DTOs;

public class ProductionAccommodationZoneDto
{
    public Guid Id { get; set; }
    public Guid ProductionAccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}