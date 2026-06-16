namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
