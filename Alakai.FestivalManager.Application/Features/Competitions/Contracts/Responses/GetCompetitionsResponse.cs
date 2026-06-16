using Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Competitions.Contracts.Responses;

public class GetCompetitionsResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
