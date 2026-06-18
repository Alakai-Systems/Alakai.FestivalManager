namespace Alakai.FestivalManager.Admin.Contracts.Competitions.Responses;

public class GetCompetitionsResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
