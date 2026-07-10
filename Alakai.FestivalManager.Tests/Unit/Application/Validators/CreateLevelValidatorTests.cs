using Alakai.FestivalManager.Application.Features.Levels.Commands.CreateLevel;
using Alakai.FestivalManager.Application.Features.Levels.Validators;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class CreateLevelValidatorTests
{
    private readonly CreateLevelValidator _sut = new();

    private static CreateLevelCommand ValidCommand() => new()
    {
        PassTypeId = Guid.NewGuid(),
        Name = "Intermediate",
        RegularPrice = 450m,
        EarlyBirdPrice = 400m,
        GroupPrice = 380m,
        SortOrder = 1
    };

    [Fact]
    public async Task Validate_WhenValid_Passes()
    {
        var result = await _sut.ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenPassTypeIdEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.PassTypeId = Guid.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.PassTypeId));
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.Name = string.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Name));
    }

    [Theory]
    [InlineData(-1)]
    public async Task Validate_WhenRegularPriceIsNegative_Fails(decimal price)
    {
        var cmd = ValidCommand(); cmd.RegularPrice = price;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.RegularPrice));
    }

    [Fact]
    public async Task Validate_WhenPriceIsZero_Passes()
    {
        var cmd = ValidCommand(); cmd.RegularPrice = 0m;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenLeaderCapacityIsNegative_Fails()
    {
        var cmd = ValidCommand(); cmd.LeaderCapacity = -1;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.LeaderCapacity));
    }
}