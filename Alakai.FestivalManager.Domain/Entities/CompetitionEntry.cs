namespace Alakai.FestivalManager.Domain.Entities;

public class CompetitionEntry : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public Guid? PartnerRegistrationId { get; set; }
    public Registration? PartnerRegistration { get; set; }

    public DanceRole? DanceRole { get; set; }

    public CompetitionFormat Format { get; set; }
    public MixAndMtachLevel? MixAndMatchLevel { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public CompetitionEntryStatus Status { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;
}
