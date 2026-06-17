namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public interface IDiscountCalculationService
{
    Task<DiscountCalculationResult> CalculateAsync(Guid editionId, decimal basePrice, string? discountCodeValue, CancellationToken cancellationToken = default);
}