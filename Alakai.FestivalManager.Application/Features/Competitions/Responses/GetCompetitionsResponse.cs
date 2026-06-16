namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class GetCompetitionsResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
