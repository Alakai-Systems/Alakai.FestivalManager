namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class CreateAccommodationRequest
{
    public Guid AccommodationZoneId { get; set; }
    public string Names { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
