namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.CreateAccommodationBuilding;

public class CreateAccommodationBuildingCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}
