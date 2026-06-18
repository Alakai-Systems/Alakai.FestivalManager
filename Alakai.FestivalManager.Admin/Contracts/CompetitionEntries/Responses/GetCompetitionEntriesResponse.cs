namespace Alakai.FestivalManager.Admin.Contracts.CompetitionEntries.Responses;

public class GetCompetitionEntriesResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
