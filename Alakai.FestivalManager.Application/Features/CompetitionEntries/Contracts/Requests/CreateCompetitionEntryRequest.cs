namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Requests;

public class CreateCompetitionEntryRequest
{
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public Guid CompetitionCapacityId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}