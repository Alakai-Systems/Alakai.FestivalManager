namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesByRegistrationIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
