namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public class DiscountCalculationService : IDiscountCalculationService
{
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public DiscountCalculationService(
        IDiscountCodeRepository discountCodeRepository)
    {
        _discountCodeRepository = discountCodeRepository;
    }

    public async Task<DiscountCalculationResult> CalculateAsync(Guid editionId, decimal basePrice, string? discountCodeValue, CancellationToken cancellationToken = default)
    {
        DiscountCalculationResult result = new()
        {
            FinalPrice = basePrice,
            DiscountStatus = DiscountApplicationStatus.None
        };

        if (string.IsNullOrEmpty(discountCodeValue))
        {
            return result;
        }

        string normalizedCode = discountCodeValue.Trim().ToUpperInvariant();

        DiscountCode? entity = await _discountCodeRepository.GetByEditionAndCodeAsync(editionId, discountCodeValue, cancellationToken);

        if (entity is null)
        {
            entity = new DiscountCode
            {
                EditionId = editionId,
                Code = normalizedCode,
                Name = normalizedCode,
                Description = "Automatically created group discount code from registration.",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 20,
                ActivationType = DiscountActivationType.AfterThreshold,
                ActivationThreshold = 10,
                CurrentUses = 0,
                IsActive = true
            };

            await _discountCodeRepository.AddAsync(entity, cancellationToken);
            await _discountCodeRepository.SaveChangesAsync(cancellationToken);
        }

        if (entity.IsActive is false)
        {
            return result;
        }

        if (entity.StartsAt.HasValue && entity.StartsAt.Value > DateTime.UtcNow)
        {
            return result;
        }

        if (entity.EndsAt.HasValue && entity.EndsAt.Value < DateTime.UtcNow)
        {
            return result;
        }

        result.DiscountCodeId = entity.Id;
        result.DiscountCodeValue = entity.Code;

        if (entity.ActivationType == DiscountActivationType.AfterThreshold)
        {
            if (entity.CurrentUses + 1 < entity.ActivationThreshold)
            {
                result.DiscountStatus = DiscountApplicationStatus.PendingThreshold;
                result.DiscountCodeId = entity.Id;
                result.DiscountCodeValue = entity.Code;
                result.FinalPrice = basePrice;
                return result;
            }
        }

        decimal discountAmount = entity.DiscountType == DiscountType.Percentage
            ? basePrice * entity.DiscountValue / 100m
            : entity.DiscountValue;

        if (discountAmount > basePrice)
        {
            discountAmount = basePrice;
        }

        result.DiscountAmount = discountAmount;
        result.FinalPrice = basePrice - discountAmount;
        result.DiscountStatus = DiscountApplicationStatus.Applied;

        return result;
    }
}