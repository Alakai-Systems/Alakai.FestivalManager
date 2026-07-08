namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;

public class GetAccommodationBuildingsResponse
{
    public IReadOnlyList<AccommodationBuildingSummaryDto> Buildings { get; set; } = [];
}
