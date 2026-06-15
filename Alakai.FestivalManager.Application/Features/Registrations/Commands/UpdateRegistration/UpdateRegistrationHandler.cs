using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.UpdateRegistration;

public class UpdateRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IMapper _mapper;

    public UpdateRegistrationHandler(IRegistrationRepository registrationRepository, IEditionRepository editionRepository, IPassTypeRepository passTypeRepository, ILevelRepository levelRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _editionRepository = editionRepository;
        _passTypeRepository= passTypeRepository;
        _levelRepository = levelRepository;
        _mapper = mapper;
    }

    public async Task<RegistrationDto> HandleAsync(UpdateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? existing = await _registrationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Registration with id '{command.Id}' was not found.");
        }

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

        // Dup check: if email changed, ensure no duplicate in edition
        if (!string.Equals(existing.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool exists = await _registrationRepository.ExistsByEditionAndEmailAsync(command.EditionId, command.Email, cancellationToken);

            if (exists)
            {
                throw new BusinessRuleException($"A registration with email '{command.Email}' already exists for this edition.");
            }
        }

        if (command.PartnerRegistrationId.HasValue)
        {
            Registration? partnerRegistration = await _registrationRepository.GetByIdAsync(command.PartnerRegistrationId.Value, cancellationToken);

            if (partnerRegistration is null)
            {
                throw new NotFoundException($"Partner registration with id '{command.PartnerRegistrationId}' was not found.");
            }
        }

        _mapper.Map(command, existing);

        existing.SetUpdated();
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        RegistrationDto dto = _mapper.Map<RegistrationDto>(existing);

        return dto;
    }
}
