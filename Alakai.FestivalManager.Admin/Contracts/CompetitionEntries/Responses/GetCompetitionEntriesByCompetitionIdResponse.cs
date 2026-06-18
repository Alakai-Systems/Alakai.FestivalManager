namespace Alakai.FestivalManager.Admin.Contracts.CompetitionEntries.Responses;

public class GetCompetitionEntriesByCompetitionIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
