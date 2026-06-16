using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

public class GetCompetitionEntriesByRegistrationIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
