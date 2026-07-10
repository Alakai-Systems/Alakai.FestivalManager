namespace Alakai.FestivalManager.Tests.Unit.Application.Features.DiscountCodes;

public class DiscountCalculationServiceTests
{
    private readonly Mock<IDiscountCodeRepository> _discountCodeRepo = new();
    private readonly DiscountCalculationService _sut;

    public DiscountCalculationServiceTests()
    {
        _sut = new DiscountCalculationService(_discountCodeRepo.Object);
    }

    // ── No discount code ──────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_WhenNoCodeProvided_ReturnsFinalPriceEqualToBasePrice()
    {
        decimal basePrice = 450m;

        DiscountCalculationResult result = await _sut.CalculateAsync(Guid.NewGuid(), basePrice, null);

        result.FinalPrice.Should().Be(basePrice);
        result.DiscountAmount.Should().Be(0m);
        result.DiscountStatus.Should().Be(DiscountApplicationStatus.None);
    }

    [Fact]
    public async Task CalculateAsync_WhenEmptyCodeProvided_ReturnsFinalPriceEqualToBasePrice()
    {
        decimal basePrice = 450m;

        DiscountCalculationResult result = await _sut.CalculateAsync(Guid.NewGuid(), basePrice, string.Empty);

        result.FinalPrice.Should().Be(basePrice);
        result.DiscountStatus.Should().Be(DiscountApplicationStatus.None);
    }

    // ── Percentage discount ───────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_WhenPercentageCodeIsValid_AppliesCorrectDiscount()
    {
        Guid editionId = Guid.NewGuid();
        decimal basePrice = 400m;
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            discountType: DiscountType.Percentage,
            discountValue: 10m,
            isActive: true);

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, basePrice, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.Applied);
        result.DiscountAmount.Should().Be(40m);    // 10% of 400
        result.FinalPrice.Should().Be(360m);
    }

    // ── Fixed amount discount ─────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_WhenFixedAmountCodeIsValid_AppliesCorrectDiscount()
    {
        Guid editionId = Guid.NewGuid();
        decimal basePrice = 450m;
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            discountType: DiscountType.FixedAmount,
            discountValue: 20m,
            isActive: true);

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, basePrice, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.Applied);
        result.DiscountAmount.Should().Be(20m);
        result.FinalPrice.Should().Be(430m);
    }

    [Fact]
    public async Task CalculateAsync_WhenFixedAmountExceedsBasePrice_CapsDiscountAtBasePrice()
    {
        Guid editionId = Guid.NewGuid();
        decimal basePrice = 10m;
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            discountType: DiscountType.FixedAmount,
            discountValue: 50m,
            isActive: true);

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, basePrice, code.Code);

        result.FinalPrice.Should().Be(0m);
        result.DiscountAmount.Should().Be(basePrice);
    }

    // ── Inactive / expired codes ──────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_WhenCodeIsInactive_ReturnsNoDiscount()
    {
        Guid editionId = Guid.NewGuid();
        DiscountCode code = Builders.BuildDiscountCode(editionId: editionId, isActive: false);

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, 450m, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.None);
        result.FinalPrice.Should().Be(450m);
    }

    [Fact]
    public async Task CalculateAsync_WhenCodeHasNotStartedYet_ReturnsNoDiscount()
    {
        Guid editionId = Guid.NewGuid();
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            isActive: true,
            startsAt: DateTime.UtcNow.AddDays(1));

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, 450m, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.None);
    }

    [Fact]
    public async Task CalculateAsync_WhenCodeHasExpired_ReturnsNoDiscount()
    {
        Guid editionId = Guid.NewGuid();
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            isActive: true,
            endsAt: DateTime.UtcNow.AddDays(-1));

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, 450m, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.None);
    }

    // ── Threshold activation ──────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_WhenThresholdNotYetReached_ReturnsPendingStatus()
    {
        Guid editionId = Guid.NewGuid();
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            isActive: true,
            activationType: DiscountActivationType.AfterThreshold,
            activationThreshold: 10,
            currentUses: 5);   // 5+1=6 < 10 → pending

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, 450m, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.PendingThreshold);
        result.FinalPrice.Should().Be(450m);
    }

    [Fact]
    public async Task CalculateAsync_WhenThresholdReached_AppliesDiscount()
    {
        Guid editionId = Guid.NewGuid();
        DiscountCode code = Builders.BuildDiscountCode(
            editionId: editionId,
            isActive: true,
            discountType: DiscountType.FixedAmount,
            discountValue: 20m,
            activationType: DiscountActivationType.AfterThreshold,
            activationThreshold: 10,
            currentUses: 9);   // 9+1=10 >= 10 → apply

        _discountCodeRepo.Setup(r => r.GetByEditionAndCodeAsync(editionId, code.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        DiscountCalculationResult result = await _sut.CalculateAsync(editionId, 450m, code.Code);

        result.DiscountStatus.Should().Be(DiscountApplicationStatus.Applied);
        result.FinalPrice.Should().Be(430m);
    }
}
