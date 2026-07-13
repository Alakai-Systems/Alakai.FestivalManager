using Alakai.FestivalManager.Application.Features.Auth.Contracts.DTOs;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Auth;

public class AuthServiceLoginTests
{
    private readonly Mock<IAuthRepository> _authRepo = new();
    private readonly Mock<IPasswordService> _passwordSvc = new();
    private readonly Mock<IJwtService> _jwtSvc = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IValidator<LoginCommand>> _loginValidator = new();
    private readonly Mock<IValidator<RefreshTokenCommand>> _refreshValidator = new();
    private readonly Mock<IValidator<ForgotPasswordCommand>> _forgotValidator = new();
    private readonly Mock<IValidator<ResetPasswordCommand>> _resetValidator = new();
    private readonly Mock<IValidator<ChangePasswordCommand>> _changeValidator = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmailNotificationService> _emailSvc = new();
    private readonly Mock<IExternalAuthService> _externalAuth = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly AuthService _sut;

    public AuthServiceLoginTests()
    {
        _sut = new AuthService(
            _authRepo.Object, _passwordSvc.Object, _jwtSvc.Object, _mapper.Object,
            _loginValidator.Object, _refreshValidator.Object, _forgotValidator.Object,
            _resetValidator.Object, _changeValidator.Object, _userRepo.Object,
            _emailSvc.Object, _externalAuth.Object, _registrationRepo.Object, _logger.Object);

        _loginValidator.Setup(v => v.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private static User BuildActiveUser(string email = "mago@alakai.com") => new()
    {
        Email = email,
        IsActive = true,
        IsLocked = false,
        PasswordHash = "hashed",
        FailedLoginAttempts = 0
    };

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsFailure()
    {
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        ApiResponse<LoginResponse> result = await _sut.LoginAsync(new LoginCommand { Email = "x@x.com", Password = "password1" });

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid email or password"));
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ReturnsFailure()
    {
        User user = BuildActiveUser();
        user.IsActive = false;
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        ApiResponse<LoginResponse> result = await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "password1" });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsLocked_ReturnsLockedMessage()
    {
        User user = BuildActiveUser();
        user.IsLocked = true;
        user.LockoutEndAt = DateTime.UtcNow.AddMinutes(10);
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        ApiResponse<LoginResponse> result = await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "password1" });

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("locked"));
    }

    [Fact]
    public async Task LoginAsync_WhenLockoutExpired_ProceedsWithLogin()
    {
        User user = BuildActiveUser();
        user.IsLocked = true;
        user.LockoutEndAt = DateTime.UtcNow.AddMinutes(-1); // expired
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordSvc.Setup(p => p.VerifyPassword(user, It.IsAny<string>(), user.PasswordHash))
            .Returns(true);
        _jwtSvc.Setup(j => j.GenerateRefreshToken(user)).Returns(new RefreshToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(7) });
        _jwtSvc.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _mapper.Setup(m => m.Map<AuthResultDto>(It.IsAny<object>())).Returns(new AuthResultDto());

        ApiResponse<LoginResponse> result = await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "password1" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WhenWrongPassword_IncrementsFailedAttempts()
    {
        User user = BuildActiveUser();
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordSvc.Setup(p => p.VerifyPassword(user, It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "wrongpassword1" });

        user.FailedLoginAttempts.Should().Be(1);
        _authRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenFiveWrongPasswords_LocksUserFor15Minutes()
    {
        User user = BuildActiveUser();
        user.FailedLoginAttempts = 4;
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordSvc.Setup(p => p.VerifyPassword(user, It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "wrongpassword1" });

        user.IsLocked.Should().BeTrue();
        user.LockoutEndAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ResetsFailedAttemptsAndReturnsSuccess()
    {
        User user = BuildActiveUser();
        user.FailedLoginAttempts = 2;
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordSvc.Setup(p => p.VerifyPassword(user, It.IsAny<string>(), user.PasswordHash))
            .Returns(true);
        _jwtSvc.Setup(j => j.GenerateRefreshToken(user))
            .Returns(new RefreshToken { Token = "rt", ExpiresAt = DateTime.UtcNow.AddDays(7) });
        _jwtSvc.Setup(j => j.GenerateAccessToken(user)).Returns("at");
        _mapper.Setup(m => m.Map<AuthResultDto>(It.IsAny<object>())).Returns(new AuthResultDto());

        ApiResponse<LoginResponse> result = await _sut.LoginAsync(new LoginCommand { Email = user.Email, Password = "correctpassword" });

        result.Success.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_EmailIsNormalizedToLowercase()
    {
        string capturedEmail = string.Empty;
        _authRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((email, _) => capturedEmail = email)
            .ReturnsAsync((User?)null);

        await _sut.LoginAsync(new LoginCommand { Email = "MAGO@ALAKAI.COM", Password = "password1" });

        capturedEmail.Should().Be("mago@alakai.com");
    }
}