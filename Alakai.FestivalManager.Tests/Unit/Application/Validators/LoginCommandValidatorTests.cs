using Alakai.FestivalManager.Application.Features.Auth.Commands.Login;
using Alakai.FestivalManager.Application.Features.Auth.Validators;
using FluentValidation.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Alakai.FestivalManager.Tests.Unit.Application.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public async Task Validate_WhenValid_Passes()
    {
        ValidationResult result = await _sut.ValidateAsync(new LoginCommand { Email = "mago@alakai.com", Password = "password123" });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    public async Task Validate_WhenEmailInvalid_Fails(string email)
    {
        ValidationResult result = await _sut.ValidateAsync(new LoginCommand { Email = email, Password = "password123" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public async Task Validate_WhenPasswordInvalid_Fails(string password)
    {
        ValidationResult result = await _sut.ValidateAsync(new LoginCommand { Email = "mago@alakai.com", Password = password });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}