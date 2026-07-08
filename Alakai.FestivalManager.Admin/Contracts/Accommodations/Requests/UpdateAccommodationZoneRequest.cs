namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class UpdateAccommodationZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
