using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;
using Alakai.FestivalManager.Application.Features.DiscountCodes.Validators;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class CreateDiscountCodeCommandValidatorTests
{
    private readonly CreateDiscountCodeCommandValidator _sut = new();

    private static CreateDiscountCodeCommand ValidCommand() => new()
    {
        EditionId = Guid.NewGuid(),
        Code = "SWIM2026",
        Name = "Swim Out discount",
        DiscountType = DiscountType.Percentage,
        DiscountValue = 10m,
        ActivationType = DiscountActivationType.Immediate
    };

    [Fact]
    public async Task Validate_WhenValid_Passes()
    {
        var result = await _sut.ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenEditionIdEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.EditionId = Guid.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.EditionId));
    }

    [Fact]
    public async Task Validate_WhenCodeIsEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.Code = string.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Code));
    }

    [Fact]
    public async Task Validate_WhenDiscountValueIsZero_Fails()
    {
        var cmd = ValidCommand(); cmd.DiscountValue = 0m;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.DiscountValue));
    }

    [Fact]
    public async Task Validate_WhenImmediateWithThresholdValue_Fails()
    {
        var cmd = ValidCommand();
        cmd.ActivationType = DiscountActivationType.Immediate;
        cmd.ActivationThreshold = 5;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.ActivationThreshold));
    }

    [Fact]
    public async Task Validate_WhenEndsAtIsBeforeStartsAt_Fails()
    {
        var cmd = ValidCommand();
        cmd.StartsAt = DateTime.UtcNow.AddDays(5);
        cmd.EndsAt = DateTime.UtcNow.AddDays(1);
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.EndsAt));
    }

    [Fact]
    public async Task Validate_WhenMaxUsesIsZero_Fails()
    {
        var cmd = ValidCommand(); cmd.MaxUses = 0;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.MaxUses));
    }

    [Fact]
    public async Task Validate_WhenMaxUsesIsNull_Passes()
    {
        var cmd = ValidCommand(); cmd.MaxUses = null;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }
}