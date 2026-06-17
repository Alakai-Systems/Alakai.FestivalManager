namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class GetDiscountCodesResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
