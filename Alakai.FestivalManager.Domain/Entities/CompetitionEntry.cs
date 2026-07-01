namespace Alakai.FestivalManager.Domain.Entities;

public class CompetitionEntry : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public Guid? PartnerRegistrationId { get; set; }
    public Registration? PartnerRegistration { get; set; }

    public Guid CompetitionCapacityId { get; set; }
    public CompetitionCapacity CompetitionCapacity { get; set; } = default!;

    public DanceRole? DanceRole { get; set; }

    public string? TeamName { get; set; }

    // Used only for Format = Team competitions. A single CompetitionEntry represents the whole
    // team (no per-member registration); this just records how many people are in it.
    public int? TeamMemberCount { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public CompetitionEntryStatus Status { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;
}
