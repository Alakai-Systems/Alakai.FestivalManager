using Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;
using Alakai.FestivalManager.Application.Features.Registrations.Validators;
using FluentValidation.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class CreateRegistrationCommandValidatorTests
{
    private readonly CreateRegistrationCommandValidator _sut = new();

    private static CreateRegistrationCommand ValidCommand() => new()
    {
        EditionId = Guid.NewGuid(),
        PassTypeId = Guid.NewGuid(),
        FirstName = "Jose",
        LastName = "Farfan",
        Email = "jose@test.com",
        Password = "password123"
    };

    [Fact]
    public async Task Validate_WhenCommandIsValid_PassesValidation()
    {
        ValidationResult result = await _sut.ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenEditionIdIsEmpty_Fails()
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.EditionId = Guid.Empty;
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.EditionId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameIsEmpty_Fails(string? firstName)
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.FirstName = firstName!;
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.FirstName));
    }

    [Fact]
    public async Task Validate_WhenFirstNameExceedsMaxLength_Fails()
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.FirstName = new string('A', 101);
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.FirstName));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public async Task Validate_WhenEmailIsInvalid_Fails(string email)
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.Email = email;
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Email));
    }

    [Theory]
    [InlineData("short")]   // < 8 chars
    [InlineData("")]
    public async Task Validate_WhenPasswordIsInvalid_Fails(string password)
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.Password = password;
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Password));
    }

    [Fact]
    public async Task Validate_WhenPartnerEmailIsInvalid_Fails()
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.PartnerEmail = "notanemail";
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.PartnerEmail));
    }

    [Fact]
    public async Task Validate_WhenPartnerEmailIsEmpty_Passes()
    {
        CreateRegistrationCommand cmd = ValidCommand();
        cmd.PartnerEmail = string.Empty;
        ValidationResult result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }
}