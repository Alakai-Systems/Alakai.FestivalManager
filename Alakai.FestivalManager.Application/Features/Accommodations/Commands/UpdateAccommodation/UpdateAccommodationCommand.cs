namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.UpdateAccommodation;

public class UpdateAccommodationCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
