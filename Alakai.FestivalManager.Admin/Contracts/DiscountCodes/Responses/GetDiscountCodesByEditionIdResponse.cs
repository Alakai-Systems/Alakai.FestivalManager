namespace Alakai.FestivalManager.Admin.Contracts.DiscountCodes.Responses;

public class GetDiscountCodesByEditionIdResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
