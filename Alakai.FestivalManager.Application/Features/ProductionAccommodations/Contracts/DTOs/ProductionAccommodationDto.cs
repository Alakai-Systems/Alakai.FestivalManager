namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Contracts.DTOs;

public class ProductionAccommodationDto
{
    public Guid Id { get; set; }
    public Guid ProductionAccommodationZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}