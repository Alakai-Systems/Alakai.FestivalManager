namespace Alakai.FestivalManager.Application.Features.Editions.Contracts.Responses;

public class GetEditionsResponse
{
    public IReadOnlyList<EditionDto> Editions { get; set; } = [];
}