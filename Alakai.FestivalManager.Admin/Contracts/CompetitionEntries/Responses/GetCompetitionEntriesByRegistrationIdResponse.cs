namespace Alakai.FestivalManager.Admin.Contracts.CompetitionEntries.Responses;

public class GetCompetitionEntriesByRegistrationIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
