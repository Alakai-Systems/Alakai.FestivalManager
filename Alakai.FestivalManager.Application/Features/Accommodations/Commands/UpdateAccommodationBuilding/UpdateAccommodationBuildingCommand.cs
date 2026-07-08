namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.UpdateAccommodationBuilding;

public class UpdateAccommodationBuildingCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}
