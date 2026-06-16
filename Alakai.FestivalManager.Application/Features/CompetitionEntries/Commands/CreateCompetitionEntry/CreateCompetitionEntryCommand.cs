namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.CreateCompetitionEntry;

public class CreateCompetitionEntryCommand
{
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public CompetitionFormat Format { get; set; }
    public MixAndMtachLevel? MixAndMatchLevel { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}
