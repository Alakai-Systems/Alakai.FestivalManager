namespace Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;

public class PublicRegistrationAvailabilityDto
{
    public Guid EditionId { get; set; }
    public string EditionName { get; set; } = string.Empty;
    public List<PublicPassTypeDto> PassTypes { get; set; } = [];
}
