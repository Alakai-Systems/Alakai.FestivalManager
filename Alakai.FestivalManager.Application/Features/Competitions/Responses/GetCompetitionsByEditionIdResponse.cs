namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class GetCompetitionsByEditionIdResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
