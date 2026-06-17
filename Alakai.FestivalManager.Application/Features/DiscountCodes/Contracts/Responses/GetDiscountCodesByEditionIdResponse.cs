namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class GetDiscountCodesByEditionIdResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
