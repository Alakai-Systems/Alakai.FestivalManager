namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.CreateAccommodation;

public class CreateAccommodationCommand
{
    public Guid AccommodationZoneId { get; set; }
    public string Names { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
