namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;

public class DeleteRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;

    public DeleteRegistrationHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Guid> HandleAsync(DeleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Alakai.FestivalManager.Domain.Entities.Registration? existing = await _registrationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Registration with id '{command.Id}' was not found.");
        }

        _registrationRepository.Delete(existing);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
