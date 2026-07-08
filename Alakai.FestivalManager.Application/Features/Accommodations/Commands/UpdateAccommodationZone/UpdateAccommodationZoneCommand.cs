namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.UpdateAccommodationZone;

public class UpdateAccommodationZoneCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
