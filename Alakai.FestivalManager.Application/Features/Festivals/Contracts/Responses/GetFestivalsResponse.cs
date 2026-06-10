namespace Alakai.FestivalManager.Application.Features.Festivals.Contracts.Responses;
public class GetFestivalsResponse
{
    public IReadOnlyList<FestivalDto> Festivals { get; set; } = [];
}