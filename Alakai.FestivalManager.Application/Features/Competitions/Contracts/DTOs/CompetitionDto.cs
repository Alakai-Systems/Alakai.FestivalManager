namespace Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;

public class CompetitionDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompetitionFormat Format { get; set; }
    public IReadOnlyList<CompetitionCapacityDto> Capacities { get; set; } = [];
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public decimal Price { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

