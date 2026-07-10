namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Payments;

public class PaymentServiceTests
{
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly Mock<IRedsysGateway> _redsysGateway = new();
    private readonly Mock<IEmailNotificationService> _emailService = new();
    private readonly Mock<ILogger<PaymentService>> _logger = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(
            _registrationRepo.Object,
            _redsysGateway.Object,
            _emailService.Object,
            _logger.Object);
    }

    // ── CreatePaymentSessionAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CreatePaymentSessionAsync_WhenRegistrationNotFound_ReturnsFailure()
    {
        CreatePaymentSessionCommand command = new() { RegistrationId = Guid.NewGuid() };
        _registrationRepo.Setup(r => r.GetByIdAsync(command.RegistrationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        ApiResponse<RedsysPaymentFormDto> result = await _sut.CreatePaymentSessionAsync(command);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    [Fact]
    public async Task CreatePaymentSessionAsync_WhenAlreadyPaid_ReturnsFailure()
    {
        Registration registration = Builders.BuildRegistration(paymentStatus: PaymentStatus.Paid);
        CreatePaymentSessionCommand command = new() { RegistrationId = registration.Id };
        _registrationRepo.Setup(r => r.GetByIdAsync(registration.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        ApiResponse<RedsysPaymentFormDto> result = await _sut.CreatePaymentSessionAsync(command);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already paid"));
    }

    [Fact]
    public async Task CreatePaymentSessionAsync_WhenPriceIsZero_ReturnsFailure()
    {
        Registration registration = Builders.BuildRegistration(finalPrice: 0m);
        CreatePaymentSessionCommand command = new() { RegistrationId = registration.Id };
        _registrationRepo.Setup(r => r.GetByIdAsync(registration.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        ApiResponse<RedsysPaymentFormDto> result = await _sut.CreatePaymentSessionAsync(command);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("pending amount"));
    }

    [Fact]
    public async Task CreatePaymentSessionAsync_WhenValid_ReturnsSuccessAndBuildsForm()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Unpaid,
            finalPrice: 450m);
        CreatePaymentSessionCommand command = new() { RegistrationId = registration.Id };
        RedsysPaymentFormDto expectedForm = new() { Url = "https://redsys.es/tpv", Order = "ORDER123" };

        _registrationRepo.Setup(r => r.GetByIdAsync(registration.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);
        _redsysGateway.Setup(g => g.BuildPaymentForm(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(expectedForm);

        ApiResponse<RedsysPaymentFormDto> result = await _sut.CreatePaymentSessionAsync(command);

        result.Success.Should().BeTrue();
        result.Data.Should().Be(expectedForm);
        _registrationRepo.Verify(r => r.Update(It.IsAny<Registration>()), Times.Once);
        _registrationRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ProcessRedsysReturnAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenOrderNotFound_ReturnsFalse()
    {
        string params64 = BuildRedsysParams("9999999999", "0000");
        _registrationRepo.Setup(r => r.GetByOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenAlreadyPaid_ReturnsTrueWithoutReprocessing()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Paid,
            paymentReference: "ORDER123");
        string params64 = BuildRedsysParams("ORDER123", "0000");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeTrue();
        _registrationRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenAlreadyPartiallyPaid_ReturnsTrueWithoutReprocessing()
    {
        // Guard: second call to return URL should not override PartiallyPaid with Paid
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.PartiallyPaid,
            paymentPlan: PaymentPlan.SplitFiftyFifty,
            amountPaid: 225m,
            finalPrice: 450m,
            paymentReference: "ORDER123");
        string params64 = BuildRedsysParams("ORDER123", "0000");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeTrue();
        registration.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);  // unchanged
        _registrationRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenApprovedAndFullPayment_SetsStatusToPaidAndConfirmed()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentPlan: PaymentPlan.FullOnline,
            finalPrice: 450m,
            amountPaid: 0m,
            paymentReference: "ORDER123");
        string params64 = BuildRedsysParams("ORDER123", "0000", "024041");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeTrue();
        registration.PaymentStatus.Should().Be(PaymentStatus.Paid);
        registration.Status.Should().Be(RegistrationStatus.Confirmed);
        registration.AmountPaid.Should().Be(450m);
        registration.PaymentAuthCodes.Should().Contain("024041");
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenApprovedAndFirstSplitPayment_SetsPartiallyPaid()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentPlan: PaymentPlan.SplitFiftyFifty,
            finalPrice: 450m,
            amountPaid: 0m,
            paymentReference: "ORDER123");
        string params64 = BuildRedsysParams("ORDER123", "0000", "011111");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeTrue();
        registration.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        registration.AmountPaid.Should().Be(225m);    // 50% of 450
        registration.Status.Should().NotBe(RegistrationStatus.Confirmed);
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenApprovedAndSecondSplitPayment_SetsFullyPaidAndConfirmed()
    {
        // Second 50% payment: AmountPaid > 0 so goes to else branch → Paid + Confirmed
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentPlan: PaymentPlan.SplitFiftyFifty,
            finalPrice: 450m,
            amountPaid: 225m,    // first 50% already paid
            paymentReference: "ORDER456");
        string params64 = BuildRedsysParams("ORDER456", "0000", "022222");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeTrue();
        registration.PaymentStatus.Should().Be(PaymentStatus.Paid);
        registration.Status.Should().Be(RegistrationStatus.Confirmed);
        registration.AmountPaid.Should().Be(450m);
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenDeclined_SetsPaymentStatusToFailed()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentReference: "ORDER123");
        string params64 = BuildRedsysParams("ORDER123", "0190");  // 190 = declined

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        bool result = await _sut.ProcessRedsysReturnAsync(params64);

        result.Should().BeFalse();
        registration.PaymentStatus.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenApproved_StoresAuthCodeWithOrder()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentPlan: PaymentPlan.FullOnline,
            finalPrice: 450m,
            paymentReference: "ORDER789");
        string params64 = BuildRedsysParams("ORDER789", "0000", "099999");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        await _sut.ProcessRedsysReturnAsync(params64);

        registration.PaymentAuthCodes.Should().Be("ORDER789:099999");
    }

    [Fact]
    public async Task ProcessRedsysReturnAsync_WhenSecondPayment_AppendsAuthCodeWithPipeSeparator()
    {
        Registration registration = Builders.BuildRegistration(
            paymentStatus: PaymentStatus.Pending,
            paymentPlan: PaymentPlan.SplitFiftyFifty,
            finalPrice: 450m,
            amountPaid: 225m,
            paymentReference: "ORDER456");
        registration.PaymentAuthCodes = "ORDER123:011111";  // first payment already stored
        string params64 = BuildRedsysParams("ORDER456", "0000", "022222");

        _registrationRepo.Setup(r => r.GetByOrderAsync("ORDER456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        await _sut.ProcessRedsysReturnAsync(params64);

        registration.PaymentAuthCodes.Should().Be("ORDER123:011111|ORDER456:022222");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildRedsysParams(string order, string responseCode, string? authCode = null)
    {
        Dictionary<string, string> payload = new()
        {
            ["Ds_Order"] = order,
            ["Ds_Response"] = responseCode
        };

        if (authCode is not null)
        {
            payload["Ds_AuthorisationCode"] = authCode;
        }

        string json = JsonSerializer.Serialize(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
}
