namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;

public class AccommodationZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AccommodationDto> Accommodations { get; set; } = [];
}
