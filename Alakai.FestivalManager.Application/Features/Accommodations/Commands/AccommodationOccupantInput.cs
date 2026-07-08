namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands;

public class AccommodationOccupantInput
{
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
}
