using Alakai.FestivalManager.Application.Features.Editions.Commands.CreateEdition;
using Alakai.FestivalManager.Application.Features.Editions.Validators;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class CreateEditionValidatorTests
{
    private readonly CreateEditionValidator _sut = new();

    private static CreateEditionCommand ValidCommand() => new()
    {
        FestivalId = Guid.NewGuid(),
        Name = "Swim Out Costa Brava 2026",
        Year = 2026,
        StartDate = new DateTime(2026, 9, 3),
        EndDate = new DateTime(2026, 9, 7)
    };

    [Fact]
    public async Task Validate_WhenValid_Passes()
    {
        var result = await _sut.ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenFestivalIdEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.FestivalId = Guid.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.FestivalId));
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
    [InlineData(1999)]
    [InlineData(2101)]
    public async Task Validate_WhenYearOutOfRange_Fails(int year)
    {
        var cmd = ValidCommand(); cmd.Year = year;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Year));
    }

    [Fact]
    public async Task Validate_WhenStartDateAfterEndDate_Fails()
    {
        var cmd = ValidCommand();
        cmd.StartDate = new DateTime(2026, 9, 7);
        cmd.EndDate = new DateTime(2026, 9, 3);
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.StartDate));
    }

    [Fact]
    public async Task Validate_WhenRegistrationOpenAfterClose_Fails()
    {
        var cmd = ValidCommand();
        cmd.RegistrationOpenDate = new DateTime(2026, 6, 1);
        cmd.RegistrationCloseDate = new DateTime(2026, 5, 1);
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.RegistrationOpenDate));
    }

    [Fact]
    public async Task Validate_WhenRegistrationDatesAreNull_Passes()
    {
        var cmd = ValidCommand();
        cmd.RegistrationOpenDate = null;
        cmd.RegistrationCloseDate = null;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }
}