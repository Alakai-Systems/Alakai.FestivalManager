namespace Alakai.FestivalManager.Admin.Contracts.DiscountCodes.Responses;

public class GetDiscountCodesResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
