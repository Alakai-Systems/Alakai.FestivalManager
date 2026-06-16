namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompetitionFormat Format { get; set; }
    public ICollection<CompetitionCapacity> Capacities { get; set; } = [];
    public int? MaxParticipants { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public decimal Price { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public int SortOrder { get; set; }
}

