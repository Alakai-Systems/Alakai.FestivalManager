namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.UpdateProductionAccommodationZone;

public class UpdateProductionAccommodationZoneCommand
{
    public Guid Id { get; set; }
    public Guid ProductionAccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}