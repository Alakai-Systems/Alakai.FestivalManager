using Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Competitions.Contracts.Responses;

public class GetCompetitionByIdResponse
{
    public CompetitionDto Competition { get; set; } = default!;
}
