namespace Alakai.FestivalManager.Application.Features.PassTypes.Contracts.Responses;

public class GetPassTypeByIdResponse
{
    public PassTypeDto PassType { get; set; } = default!;
}