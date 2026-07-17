using Alakai.FestivalManager.Application.Features.Emails.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.DiscountCodes.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Services;
using Alakai.FestivalManager.Application.Services.Security;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Registrations;

public class CreateRegistrationHandlerTests
{
    private readonly Mock<IRegistrationRepository> _regRepo = new();
    private readonly Mock<IEditionRepository> _editionRepo = new();
    private readonly Mock<IPassTypeRepository> _passTypeRepo = new();
    private readonly Mock<ILevelRepository> _levelRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmailNotificationService> _emailSvc = new();
    private readonly Mock<IDiscountCalculationService> _discountSvc = new();
    private readonly Mock<IDiscountCodeRepository> _discountRepo = new();
    private readonly Mock<IPasswordHasherService> _passwordHasher = new();
    private readonly Mock<IRegistrationPartnerService> _partnerSvc = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IBackgroundTaskQueue> _backgroundTaskQueue = new();
    private readonly CreateRegistrationHandler _sut;

    private readonly Edition _edition = new() { IsActive = true };
    private readonly PassType _passType = new();
    private readonly Level _level = new() { RegularPrice = 450m };

    public CreateRegistrationHandlerTests()
    {
        _sut = new CreateRegistrationHandler(
            _regRepo.Object, _editionRepo.Object, _passTypeRepo.Object, _levelRepo.Object,
            _userRepo.Object, _mapper.Object, _emailSvc.Object, _discountSvc.Object,
            _discountRepo.Object, _passwordHasher.Object, _partnerSvc.Object, _backgroundTaskQueue.Object);

        // Defaults: happy path
        _editionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(_edition);
        _passTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(_passType);
        _regRepo.Setup(r => r.ExistsByEditionAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasher.Setup(p => p.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("hashed");
        _discountSvc.Setup(d => d.CalculateAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiscountCalculationResult { FinalPrice = 450m, DiscountStatus = DiscountApplicationStatus.None });
        _mapper.Setup(m => m.Map<Registration>(It.IsAny<object>())).Returns(new Registration { Email = "test@test.com" });
        _mapper.Setup(m => m.Map<RegistrationDto>(It.IsAny<Registration>())).Returns(new RegistrationDto());
        _partnerSvc.Setup(p => p.LinkPartnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }

    private CreateRegistrationCommand BuildCommand(Guid? levelId = null) => new()
    {
        EditionId = Guid.NewGuid(),
        PassTypeId = Guid.NewGuid(),
        LevelId = levelId,
        FirstName = "Jose",
        LastName = "Farfan",
        Email = "jose@test.com",
        Password = "password123"
    };

    [Fact]
    public async Task HandleAsync_WhenEditionNotFound_ThrowsNotFoundException()
    {
        _editionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Edition?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Edition*");
    }

    [Fact]
    public async Task HandleAsync_WhenPassTypeNotFound_ThrowsNotFoundException()
    {
        _passTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PassType?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Pass type*");
    }

    [Fact]
    public async Task HandleAsync_WhenLevelDoesNotBelongToPassType_ThrowsBusinessRuleException()
    {
        Guid passTypeId = Guid.NewGuid();
        Level level = new() { PassTypeId = Guid.NewGuid() }; // different passType
        _levelRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(level);

        CreateRegistrationCommand command = BuildCommand(levelId: Guid.NewGuid());
        command.PassTypeId = passTypeId;

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Level does not belong*");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyRegisteredInEdition_ThrowsBusinessRuleException()
    {
        _regRepo.Setup(r => r.ExistsByEditionAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_CreatesNewUser()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await _sut.HandleAsync(BuildCommand());

        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasher.Verify(p => p.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyExists_ReusesExistingUser()
    {
        User existingUser = new() { Email = "jose@test.com", IsActive = true };
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        await _sut.HandleAsync(BuildCommand());

        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDeferredTenDays_SetsPaymentDueAtTenDaysFromNow()
    {
        Registration captured = new() { Email = "test@test.com" };
        _mapper.Setup(m => m.Map<Registration>(It.IsAny<object>())).Returns(captured);

        CreateRegistrationCommand command = BuildCommand();
        command.PaymentPlan = PaymentPlan.DeferredTenDays;

        await _sut.HandleAsync(command);

        captured.PaymentDueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_WhenSplitFiftyFifty_SetsPaymentDueAt30Days()
    {
        Registration captured = new() { Email = "test@test.com" };
        _mapper.Setup(m => m.Map<Registration>(It.IsAny<object>())).Returns(captured);

        CreateRegistrationCommand command = BuildCommand();
        command.PaymentPlan = PaymentPlan.SplitFiftyFifty;

        await _sut.HandleAsync(command);

        captured.PaymentDueAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_WhenFullOnline_PaymentDueAtIsNull()
    {
        Registration captured = new() { Email = "test@test.com" };
        _mapper.Setup(m => m.Map<Registration>(It.IsAny<object>())).Returns(captured);

        CreateRegistrationCommand command = BuildCommand();
        command.PaymentPlan = PaymentPlan.FullOnline;

        await _sut.HandleAsync(command);

        captured.PaymentDueAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_SavesRegistrationAndSendsEmail()
    {
        await _sut.HandleAsync(BuildCommand());

        _regRepo.Verify(r => r.AddAsync(It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Once);
        _regRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSvc.Verify(e => e.CreateAndSendEmailAsync(
            EmailTemplateKey.RegistrationCreated, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}