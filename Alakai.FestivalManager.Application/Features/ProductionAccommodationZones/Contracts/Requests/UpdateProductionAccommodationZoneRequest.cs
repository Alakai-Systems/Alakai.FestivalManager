namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Contracts.Requests;

public class UpdateProductionAccommodationZoneRequest
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}