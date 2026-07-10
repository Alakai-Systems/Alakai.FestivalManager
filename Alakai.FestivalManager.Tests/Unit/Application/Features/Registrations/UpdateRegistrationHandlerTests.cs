using Alakai.FestivalManager.Application.Features.Registrations.Commands.UpdateRegistration;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.DiscountCodes.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Services;
using Alakai.FestivalManager.Tests.Unit.Application.Common;
using AutoMapper;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Registrations;

public class UpdateRegistrationHandlerTests
{
    private readonly Mock<IRegistrationRepository> _regRepo = new();
    private readonly Mock<IEditionRepository> _editionRepo = new();
    private readonly Mock<IPassTypeRepository> _passTypeRepo = new();
    private readonly Mock<ILevelRepository> _levelRepo = new();
    private readonly Mock<IDiscountCodeRepository> _discountRepo = new();
    private readonly Mock<IDiscountCalculationService> _discountSvc = new();
    private readonly Mock<IRegistrationPartnerService> _partnerSvc = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateRegistrationHandler _sut;

    public UpdateRegistrationHandlerTests()
    {
        _sut = new UpdateRegistrationHandler(
            _regRepo.Object, _editionRepo.Object, _passTypeRepo.Object, _levelRepo.Object,
            _mapper.Object, _discountSvc.Object, _discountRepo.Object, _partnerSvc.Object);

        _editionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Edition());
        _passTypeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PassType());
        _discountSvc.Setup(d => d.CalculateAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiscountCalculationResult { FinalPrice = 450m, DiscountStatus = DiscountApplicationStatus.None });
        _mapper.Setup(m => m.Map(It.IsAny<UpdateRegistrationCommand>(), It.IsAny<Registration>())).Returns(new Registration());
        _mapper.Setup(m => m.Map<RegistrationDto>(It.IsAny<Registration>())).Returns(new RegistrationDto());
        _partnerSvc.Setup(p => p.LinkPartnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _discountRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _regRepo.Setup(r => r.ExistsByEditionAndEmailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }

    private UpdateRegistrationCommand BuildCommand(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EditionId = Guid.NewGuid(),
        PassTypeId = Guid.NewGuid(),
        Email = "jose@test.com",
        PaymentStatus = PaymentStatus.Unpaid
    };

    [Fact]
    public async Task HandleAsync_WhenRegistrationNotFound_ThrowsNotFoundException()
    {
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Registration?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentStatusSetToPaid_SetsStatusToConfirmed()
    {
        Registration reg = Builders.BuildRegistration(status: RegistrationStatus.PendingPayment);
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        UpdateRegistrationCommand command = BuildCommand();
        command.PaymentStatus = PaymentStatus.Paid;

        await _sut.HandleAsync(command);

        reg.Status.Should().Be(RegistrationStatus.Confirmed);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentStatusNotPaid_DoesNotChangeStatus()
    {
        Registration reg = Builders.BuildRegistration(status: RegistrationStatus.PendingPayment);
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        UpdateRegistrationCommand command = BuildCommand();
        command.PaymentStatus = PaymentStatus.Unpaid;

        await _sut.HandleAsync(command);

        reg.Status.Should().Be(RegistrationStatus.PendingPayment);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailChangedToDuplicate_ThrowsBusinessRuleException()
    {
        Registration reg = Builders.BuildRegistration();
        reg.Email = "old@test.com";
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _regRepo.Setup(r => r.ExistsByEditionAndEmailAsync(It.IsAny<Guid>(), "new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        UpdateRegistrationCommand command = BuildCommand();
        command.Email = "new@test.com";

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task HandleAsync_WhenLevelDoesNotBelongToPassType_ThrowsBusinessRuleException()
    {
        Registration reg = Builders.BuildRegistration();
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _levelRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Level { PassTypeId = Guid.NewGuid() });

        UpdateRegistrationCommand command = BuildCommand();
        command.LevelId = Guid.NewGuid();
        command.PassTypeId = Guid.NewGuid();

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*does not belong*");
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_SavesChanges()
    {
        Registration reg = Builders.BuildRegistration();
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        await _sut.HandleAsync(BuildCommand());

        _regRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}