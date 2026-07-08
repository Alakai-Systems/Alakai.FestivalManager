namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.CreateAccommodationZone;

public class CreateAccommodationZoneCommand
{
    public Guid AccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
