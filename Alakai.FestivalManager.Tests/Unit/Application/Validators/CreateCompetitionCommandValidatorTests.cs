using Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;
using Alakai.FestivalManager.Application.Features.Competitions.Validators;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class CreateCompetitionCommandValidatorTests
{
    private readonly CreateCompetitionCommandValidator _sut = new();

    private static CreateCompetitionCommand ValidCommand() => new()
    {
        EditionId = Guid.NewGuid(),
        Name = "Lindy Hop Jack & Jill",
        Price = 20m,
        SortOrder = 1,
        Format = CompetitionFormat.Partnered
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
    public async Task Validate_WhenNameIsEmpty_Fails()
    {
        var cmd = ValidCommand(); cmd.Name = string.Empty;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Name));
    }

    [Fact]
    public async Task Validate_WhenPriceIsNegative_Fails()
    {
        var cmd = ValidCommand(); cmd.Price = -1m;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Price));
    }

    [Fact]
    public async Task Validate_WhenMaxParticipantsIsZero_Fails()
    {
        var cmd = ValidCommand(); cmd.MaxParticipants = 0;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.MaxParticipants));
    }

    [Fact]
    public async Task Validate_WhenMaxParticipantsIsNull_Passes()
    {
        var cmd = ValidCommand(); cmd.MaxParticipants = null;
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }
}