namespace Alakai.FestivalManager.Admin.Contracts.ProductionZones.Requests;

public class CreateProductionAccommodationZoneRequest
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}