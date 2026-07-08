namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public interface IPublicRegistrationService
{
    Task<PublicRegistrationAvailabilityDto> GetAvailabilityAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<PublicDiscountCheckDto> CheckDiscountCodeAsync(Guid editionId, string code, decimal basePrice, CancellationToken cancellationToken = default);
    Task<PublicPartnerLookupDto> LookupPartnerAsync(Guid editionId, string email, CancellationToken cancellationToken = default);
}
