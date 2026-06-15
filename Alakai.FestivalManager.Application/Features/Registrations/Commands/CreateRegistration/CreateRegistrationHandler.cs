namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;

public class CreateRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IMapper _mapper;

    public CreateRegistrationHandler(IRegistrationRepository registrationRepository, IEditionRepository editionRepository, IPassTypeRepository passTypeRepository, ILevelRepository levelRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _editionRepository = editionRepository;
        _passTypeRepository = passTypeRepository;
        _levelRepository = levelRepository;
        _mapper = mapper;
    }

    public async Task<RegistrationDto> HandleAsync(CreateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        PassType? passType = await _passTypeRepository.GetByIdAsync(command.PassTypeId, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{command.PassTypeId}' was not found.");
        }

        if (command.LevelId.HasValue)
        {
            Level? level = await _levelRepository.GetByIdAsync(command.LevelId.Value, cancellationToken);

            if (level is null)
            {
                throw new NotFoundException($"Level with id '{command.LevelId}' was not found.");
            }

            if (level.PassTypeId != command.PassTypeId)
            {
                throw new BusinessRuleException("Level does not belong to the selected pass type.");
            }
        }

        bool exists = await _registrationRepository.ExistsByEditionAndEmailAsync(command.EditionId, command.Email, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A registration with email '{command.Email}' already exists for this edition.");
        }

        Registration registration = _mapper.Map<Registration>(command);
        registration.CreatedAt = DateTime.UtcNow;
        registration.Status = RegistrationStatus.PendingPayment;
        registration.PaymentStatus = PaymentStatus.Unpaid;
        registration.IsActive = true;

        await _registrationRepository.AddAsync(registration, cancellationToken);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        registration.PassType = passType;

        RegistrationDto dto = _mapper.Map<RegistrationDto>(registration);

        return dto;
    }
}
