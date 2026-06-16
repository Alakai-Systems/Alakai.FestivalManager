namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.UpdateCompetitionEntry;

public class UpdateCompetitionEntryCommand
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public CompetitionFormat Format { get; set; }
    public MixAndMtachLevel? MixAndMatchLevel { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public CompetitionEntryStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
}
