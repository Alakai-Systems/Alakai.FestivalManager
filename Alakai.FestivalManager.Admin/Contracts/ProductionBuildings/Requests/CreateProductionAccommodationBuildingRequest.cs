namespace Alakai.FestivalManager.Admin.Contracts.ProductionBuildings.Requests;

public class CreateProductionAccommodationBuildingRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
}