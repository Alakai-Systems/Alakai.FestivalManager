namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class UpdateAccommodationRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
