using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

public class GetCompetitionEntryByIdResponse
{
    public CompetitionEntryDto CompetitionEntry { get; set; } = default!;
}
