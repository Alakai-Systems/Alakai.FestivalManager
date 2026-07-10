using Alakai.FestivalManager.Application.Features.Emails.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;
using Alakai.FestivalManager.Tests.Unit.Application.Common;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Registrations;

public class DeleteRegistrationHandlerTests
{
    private readonly Mock<IRegistrationRepository> _regRepo = new();
    private readonly Mock<ICompetitionEntryRepository> _compRepo = new();
    private readonly Mock<IEmailLogRepository> _emailLogRepo = new();
    private readonly Mock<IDiscountCodeRepository> _discountRepo = new();
    private readonly Mock<IAccommodationReservationRepository> _accomRepo = new();
    private readonly Mock<IBusReservationRepository> _busRepo = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<IEmailNotificationService> _emailSvc = new();
    private readonly DeleteRegistrationHandler _sut;

    public DeleteRegistrationHandlerTests()
    {
        _sut = new DeleteRegistrationHandler(
            _regRepo.Object, _compRepo.Object, _emailLogRepo.Object, _discountRepo.Object,
            _accomRepo.Object, _busRepo.Object, _invoiceRepo.Object, _emailSvc.Object);

        _compRepo.Setup(r => r.GetByPartnerRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _compRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _accomRepo.Setup(r => r.GetByResponsibleRegistrationIdTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AccommodationReservation?)null);
        _busRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _invoiceRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Invoice?)null);
        _emailLogRepo.Setup(r => r.GetByRegistrationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationNotFound_ThrowsNotFoundException()
    {
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Registration?)null);

        Func<Task> act = () => _sut.HandleAsync(new DeleteRegistrationCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationExists_DeletesAndReturnsId()
    {
        Registration reg = Builders.BuildRegistration();
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        Guid result = await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        result.Should().Be(reg.Id);
        _regRepo.Verify(r => r.Delete(reg), Times.Once);
        _regRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSoleOccupantOfAccommodation_DeletesReservation()
    {
        Registration reg = Builders.BuildRegistration();
        AccommodationReservation reservation = Builders.BuildAccommodationReservation(
            responsibleRegistrationId: reg.Id,
            occupants: [new AccommodationReservationOccupant
            {
                RegistrationId = reg.Id, IsResponsible = true, Email = "test@test.com"
            }]);

        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _accomRepo.Setup(r => r.GetByResponsibleRegistrationIdTrackedAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        _accomRepo.Verify(r => r.Delete(reservation), Times.Once);
        _emailSvc.Verify(e => e.CreateAndSendEmailAsync(
            It.IsAny<EmailTemplateKey>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleOccupants_TransfersResponsibility()
    {
        Guid regId = Guid.NewGuid();
        Guid newRespId = Guid.NewGuid();

        AccommodationReservationOccupant oldOcc = new() { RegistrationId = regId, IsResponsible = true, Email = "old@test.com" };
        AccommodationReservationOccupant newOcc = new() { RegistrationId = newRespId, IsResponsible = false, Email = "new@test.com" };
        AccommodationReservation reservation = Builders.BuildAccommodationReservation(
            responsibleRegistrationId: regId, occupants: [oldOcc, newOcc]);

        Registration reg = Builders.BuildRegistration(id: regId);
        _regRepo.Setup(r => r.GetByIdAsync(regId, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _accomRepo.Setup(r => r.GetByResponsibleRegistrationIdTrackedAsync(regId, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        await _sut.HandleAsync(new DeleteRegistrationCommand(regId));

        _accomRepo.Verify(r => r.Delete(It.IsAny<AccommodationReservation>()), Times.Never);
        reservation.ResponsibleRegistrationId.Should().Be(newRespId);
        newOcc.IsResponsible.Should().BeTrue();
        oldOcc.IsResponsible.Should().BeFalse();
        _emailSvc.Verify(e => e.CreateAndSendEmailAsync(
            EmailTemplateKey.AccommodationNewResponsible, newRespId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenHasBusReservations_DeletesAll()
    {
        Registration reg = Builders.BuildRegistration();
        List<BusReservation> buses = [new() { RegistrationId = reg.Id }, new() { RegistrationId = reg.Id }];
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _busRepo.Setup(r => r.GetByRegistrationIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(buses);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        _busRepo.Verify(r => r.Delete(It.IsAny<BusReservation>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WhenIsPartnerInEntry_NullsOutPartnerReference()
    {
        Registration reg = Builders.BuildRegistration();
        CompetitionEntry entry = new() { PartnerRegistrationId = reg.Id };
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _compRepo.Setup(r => r.GetByPartnerRegistrationIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync([entry]);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        entry.PartnerRegistrationId.Should().BeNull();
        _compRepo.Verify(r => r.Update(entry), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenHasInvoice_DeletesInvoice()
    {
        Registration reg = Builders.BuildRegistration();
        Invoice invoice = new() { RegistrationId = reg.Id };
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _invoiceRepo.Setup(r => r.GetByRegistrationIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        _invoiceRepo.Verify(r => r.Delete(invoice), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDiscountCodeHasNoMoreUses_DeletesCode()
    {
        Guid codeId = Guid.NewGuid();
        Registration reg = Builders.BuildRegistration(discountCodeId: codeId);
        DiscountCode code = new() { ActivationType = DiscountActivationType.AfterThreshold };
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _discountRepo.Setup(r => r.GetByIdAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(code);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        _discountRepo.Verify(r => r.Delete(code), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDiscountCodeStillHasUses_DoesNotDeleteCode()
    {
        Guid codeId = Guid.NewGuid();
        Registration reg = Builders.BuildRegistration(discountCodeId: codeId);
        DiscountCode code = new() { ActivationType = DiscountActivationType.AfterThreshold };
        _regRepo.Setup(r => r.GetByIdAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        _regRepo.Setup(r => r.CountByDiscountCodeAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _discountRepo.Setup(r => r.GetByIdAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(code);

        await _sut.HandleAsync(new DeleteRegistrationCommand(reg.Id));

        _discountRepo.Verify(r => r.Delete(It.IsAny<DiscountCode>()), Times.Never);
    }
}