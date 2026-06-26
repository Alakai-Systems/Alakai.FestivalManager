namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public class RegistrationPartnerService : IRegistrationPartnerService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPartnerService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task LinkPartnerAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdWithPartnerDataAsync(registrationId, cancellationToken);

        if (registration is null || string.IsNullOrWhiteSpace(registration.PartnerEmail))
        {
            return;
        }

        Registration? partner = await _registrationRepository.GetActiveByEmailAsync(registration.PartnerEmail, cancellationToken);

        if (partner is null || partner.Id == registration.Id)
        {
            registration.PartnerRegistrationId = null;
            return;
        }

        if (partner.PartnerEmail?.Trim().ToLower() != registration.Email.Trim().ToLower())
        {
            registration.PartnerRegistrationId = null;
            return;
        }

        if (partner.PassTypeId != registration.PassTypeId)
        {
            registration.PartnerRegistrationId = null;
            return;
        }

        if (partner.LevelId != registration.LevelId)
        {
            registration.PartnerRegistrationId = null;
            return;
        }

        if (partner.DanceRole == registration.DanceRole)
        {
            registration.PartnerRegistrationId = null;
            return;
        }

        registration.PartnerRegistrationId = partner.Id;
        partner.PartnerRegistrationId = registration.Id;

        await _registrationRepository.SaveChangesAsync(cancellationToken);
    }
}
