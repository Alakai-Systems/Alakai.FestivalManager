namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.CreateProductionAccommodationZone;

public class CreateProductionAccommodationZoneCommand
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}