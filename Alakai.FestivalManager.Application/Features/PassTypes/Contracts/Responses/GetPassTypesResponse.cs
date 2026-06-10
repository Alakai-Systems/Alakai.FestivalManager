namespace Alakai.FestivalManager.Application.Features.PassTypes.Contracts.Responses;

public class GetPassTypesResponse
{
    public IReadOnlyList<PassTypeDto> PassTypes { get; set; } = [];
}
