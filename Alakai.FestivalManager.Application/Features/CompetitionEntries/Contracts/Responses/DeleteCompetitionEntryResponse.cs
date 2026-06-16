namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

public class DeleteCompetitionEntryResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
