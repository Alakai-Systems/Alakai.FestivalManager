using Alakai.FestivalManager.Application.Interfaces.Repositories;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.UpdateRegistration;

public class UpdateRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IDiscountCalculationService _discountCalculationService;
    private readonly IRegistrationPartnerService _registrationPartnerService;
    private readonly IMapper _mapper;

    public UpdateRegistrationHandler(IRegistrationRepository registrationRepository, IEditionRepository editionRepository, 
        IPassTypeRepository passTypeRepository, ILevelRepository levelRepository, IMapper mapper, IDiscountCalculationService discountCalculationService,
        IDiscountCodeRepository discountCodeRepository, IRegistrationPartnerService registrationPartnerService)
    {
        _registrationRepository = registrationRepository;
        _editionRepository = editionRepository;
        _passTypeRepository = passTypeRepository;
        _levelRepository = levelRepository;
        _mapper = mapper;
        _discountCalculationService = discountCalculationService;
        _discountCodeRepository = discountCodeRepository;
        _registrationPartnerService = registrationPartnerService;
    }

    public async Task<RegistrationDto> HandleAsync(UpdateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Level? level = new Level();
        Registration? existing = await _registrationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Registration with id '{command.Id}' was not found.");
        }

        Guid? oldCodeId = existing.DiscountCodeId;

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
            level = await _levelRepository.GetByIdAsync(command.LevelId.Value, cancellationToken);

            if (level is null)
            {
                throw new NotFoundException($"Level with id '{command.LevelId}' was not found.");
            }

            if (level.PassTypeId != command.PassTypeId)
            {
                throw new BusinessRuleException("Level does not belong to the selected pass type.");
            }
        }

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

        decimal basePrice = level.RegularPrice;
        DiscountCalculationResult discount = await _discountCalculationService.CalculateAsync(command.EditionId, basePrice, command.DiscountCodeValue, cancellationToken);

        _mapper.Map(command, existing);

        existing.DiscountCodeValue = command.DiscountCodeValue;
        existing.BasePrice = basePrice;
        existing.DiscountAmount = discount.DiscountAmount;
        existing.FinalPrice = discount.FinalPrice;
        existing.DiscountCodeId = discount.DiscountCodeId;
        existing.DiscountCodeValue = discount.DiscountCodeValue;
        existing.DiscountStatus = discount.DiscountStatus;
        existing.PaymentStatus = command.PaymentStatus;

        existing.SetUpdated();
        await _registrationRepository.SaveChangesAsync(cancellationToken);
        await _registrationPartnerService.LinkPartnerAsync(existing.Id, cancellationToken);

        // 1) Recalcular usos del código NUEVO (si hay)
        if (existing.DiscountCodeId.HasValue)
        {
            Guid newCodeId = existing.DiscountCodeId.Value;

            int uses = await _registrationRepository.CountByDiscountCodeAsync(newCodeId, cancellationToken);
            DiscountCode? newCode = await _discountCodeRepository.GetByIdAsync(newCodeId, cancellationToken);

            if (newCode is not null)
            {
                newCode.CurrentUses = uses;

                if (newCode.ActivationType != DiscountActivationType.Immediate && uses == 0)
                {
                    _discountCodeRepository.Delete(newCode);
                }
            }
        }

        // 2) Recalcular usos del código ANTIGUO si ha cambiado
        if (oldCodeId.HasValue && oldCodeId != existing.DiscountCodeId)
        {
            int oldUses = await _registrationRepository.CountByDiscountCodeAsync(oldCodeId.Value, cancellationToken);
            DiscountCode? oldCode = await _discountCodeRepository.GetByIdAsync(oldCodeId.Value, cancellationToken);

            if (oldCode is not null)
            {
                oldCode.CurrentUses = oldUses;

                if (oldCode.ActivationType != DiscountActivationType.Immediate && oldUses == 0)
                {
                    _discountCodeRepository.Delete(oldCode);
                }
            }
        }

        await _discountCodeRepository.SaveChangesAsync(cancellationToken);


        RegistrationDto dto = _mapper.Map<RegistrationDto>(existing);

        return dto;
    }
}
