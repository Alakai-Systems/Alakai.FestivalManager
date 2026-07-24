namespace Alakai.FestivalManager.Admin.Contracts.ProductionAccommodations.Requests;

public class UpdateProductionAccommodationRequest
{
    public Guid ProductionAccommodationZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}