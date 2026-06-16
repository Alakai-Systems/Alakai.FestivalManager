using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

public class GetCompetitionEntriesByCompetitionIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
