namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class CreateAccommodationBuildingRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; } = 1;
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}
