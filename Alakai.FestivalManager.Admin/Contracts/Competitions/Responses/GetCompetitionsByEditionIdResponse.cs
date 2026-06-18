namespace Alakai.FestivalManager.Admin.Contracts.Competitions.Responses;

public class GetCompetitionsByEditionIdResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
