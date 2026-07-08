namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class UpdateAccommodationBuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}
