using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

public class UpdateCompetitionEntryResponse
{
    public CompetitionEntryDto CompetitionEntry { get; set; } = default!;
}
