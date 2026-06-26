namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public interface IRegistrationPartnerService
{
    Task LinkPartnerAsync(Guid registrationId, CancellationToken cancellationToken = default);
}
