namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class CreateAccommodationZoneRequest
{
    public Guid AccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
