using Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;

public class CompetitionEntryDto
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public Guid CompetitionCapacityId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public CompetitionFormat Format { get; set; }
    public IReadOnlyList<CompetitionCapacityDto> Capacities { get; set; } = [];
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public CompetitionEntryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
}

