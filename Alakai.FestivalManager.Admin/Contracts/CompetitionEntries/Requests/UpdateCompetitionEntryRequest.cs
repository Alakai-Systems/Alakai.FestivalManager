namespace Alakai.FestivalManager.Admin.Contracts.CompetitionEntries.Requests;

public class UpdateCompetitionEntryRequest
{
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public Guid CompetitionCapacityId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public CompetitionEntryStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
}
