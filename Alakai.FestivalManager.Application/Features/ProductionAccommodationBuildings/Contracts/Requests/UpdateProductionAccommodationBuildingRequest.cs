namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Contracts.Requests;

public class UpdateProductionAccommodationBuildingRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}