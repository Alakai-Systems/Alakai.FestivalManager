using Alakai.FestivalManager.Application.Features.Emails.Services;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;

public class CreateRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IDiscountCalculationService _discountCalculationService;
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IRegistrationPartnerService _registrationPartnerService;
    private readonly IMapper _mapper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public CreateRegistrationHandler(IRegistrationRepository registrationRepository, IEditionRepository editionRepository, 
        IPassTypeRepository passTypeRepository, ILevelRepository levelRepository, IUserRepository userRepository, IMapper mapper,
        IEmailNotificationService emailNotificationService, IDiscountCalculationService discountCalculationService,
        IDiscountCodeRepository discountCodeRepository, IPasswordHasherService passwordHasherService, IRegistrationPartnerService registrationPartnerService,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _registrationRepository = registrationRepository;
        _editionRepository = editionRepository;
        _passTypeRepository = passTypeRepository;
        _levelRepository = levelRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _emailNotificationService = emailNotificationService;
        _discountCalculationService = discountCalculationService;
        _discountCodeRepository = discountCodeRepository;
        _passwordHasherService = passwordHasherService;
        _registrationPartnerService = registrationPartnerService;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public async Task<RegistrationDto> HandleAsync(CreateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Level level = new();
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
            level = await _levelRepository.GetByIdAsync(command.LevelId.Value, cancellationToken) ?? new Level();

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

        User? user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                Phone = command.Phone,
                Country = command.Country,
                City = command.City,
                PasswordHash = string.Empty,
                MustChangePassword = false,
                IsActive = true
            };

            user.PasswordHash = _passwordHasherService.HashPassword(user, command.Password);

            await _userRepository.AddAsync(user, cancellationToken);
        }

        decimal basePrice = level.RegularPrice;

        DiscountCalculationResult discount = await _discountCalculationService.CalculateAsync(command.EditionId, basePrice, command.DiscountCodeValue, cancellationToken);

        Registration registration = _mapper.Map<Registration>(command);
        registration.UserId = user.Id;
        registration.Status = RegistrationStatus.PendingPayment;
        registration.PaymentStatus = PaymentStatus.Unpaid;
        registration.IsActive = true;
        registration.BasePrice = basePrice;
        registration.DiscountAmount = discount.DiscountAmount;
        registration.FinalPrice = discount.FinalPrice;
        registration.DiscountCodeId = discount.DiscountCodeId;
        registration.DiscountCodeValue = discount.DiscountCodeValue;
        registration.DiscountStatus = discount.DiscountStatus;
        registration.Language = command.Language;
        registration.PaymentPlan = command.PaymentPlan;
        registration.ManagementFee = command.ManagementFee;
        registration.AmountPaid = command.AmountPaid;
        registration.PaymentDueAt = command.PaymentPlan == PaymentPlan.DeferredTenDays
            ? DateTime.UtcNow.AddDays(10)
            : command.PaymentPlan == PaymentPlan.SplitFiftyFifty
                ? DateTime.UtcNow.AddDays(30)
                : null;

        await _registrationRepository.AddAsync(registration, cancellationToken);
        await _registrationRepository.SaveChangesAsync(cancellationToken);
        await _registrationPartnerService.LinkPartnerAsync(registration.Id, cancellationToken);

        if (discount.DiscountCodeId.HasValue)
        {
            DiscountCode? code = await _discountCodeRepository.GetByIdAsync(discount.DiscountCodeId.Value, cancellationToken);

            if (code is not null)
            {
                int uses = await _registrationRepository.CountByDiscountCodeAsync(code.Id, cancellationToken);
                code.CurrentUses = uses;

                await _discountCodeRepository.SaveChangesAsync(cancellationToken);
            }
        }

        Guid registrationIdForEmail = registration.Id;

        _backgroundTaskQueue.QueueBackgroundWorkItem(async (serviceProvider, ct) =>
        {
            IEmailNotificationService scopedEmailService = serviceProvider.GetRequiredService<IEmailNotificationService>();
            await scopedEmailService.CreateAndSendEmailAsync(EmailTemplateKey.RegistrationCreated, registrationIdForEmail, ct);
        });

        RegistrationDto dto = _mapper.Map<RegistrationDto>(registration);

        return dto;
    }
}

