namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesByCompetitionIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
