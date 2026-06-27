namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginCommand> _loginValidator;
    private readonly IValidator<RefreshTokenCommand> _refreshTokenValidator;
    private readonly IValidator<ForgotPasswordCommand> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordCommand> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordCommand> _changePasswordValidator;

    public AuthService(IAuthRepository authRepository, IPasswordService passwordService, IJwtService jwtService, IMapper mapper, 
        IValidator<LoginCommand> loginValidator, IValidator<RefreshTokenCommand> refreshTokenValidator, IValidator<ForgotPasswordCommand> forgotPasswordValidator, 
        IValidator<ResetPasswordCommand> resetPasswordValidator, IValidator<ChangePasswordCommand> changePasswordValidator, IUserRepository userRepository)
    {
        _authRepository = authRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _mapper = mapper;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _loginValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        string normalizedEmail = command.Email.Trim().ToLowerInvariant();
        User? user = await _authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            ApiResponse<LoginResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Invalid email or password."],
                Message = "Login failed"
            };

            return apiResponse;
        }

        if (user.IsLocked && user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
        {
            ApiResponse<LoginResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["User is temporarily locked."],
                Message = "Login failed"
            };

            return apiResponse;
        }

        bool validPassword = _passwordService.VerifyPassword(user, command.Password, user.PasswordHash);

        if (validPassword is false)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
            }

            await _authRepository.SaveChangesAsync(cancellationToken);

            ApiResponse<LoginResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Invalid email or password."],
                Message = "Login failed"
            };

            return apiResponse;
        }

        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEndAt = null;
        user.LastLoginAt = DateTime.UtcNow;

        RefreshToken refreshToken = _jwtService.GenerateRefreshToken(user);
        await _authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        AuthResultDto authResult = CreateAuthResult(user, refreshToken.Token);

        ApiResponse<LoginResponse> apiResponseSuccess = new()
        {
            Success = true,
            Data = new LoginResponse { Auth = authResult },
            Errors = [],
            Message = "Login successful"
        };

        return apiResponseSuccess;
    }

    public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _refreshTokenValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            ApiResponse<RefreshTokenResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                Message = "Validation failed"
            };

            return apiResponse;
        }

        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromExpiredToken(command.AccessToken);

        if (principal is null)
        {
            ApiResponse<RefreshTokenResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Invalid access token."],
                Message = "Refresh token failed"
            };

            return apiResponse;
        }

        string? userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            ApiResponse<RefreshTokenResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Invalid access token."],
                Message = "Refresh token failed"
            };

            return apiResponse;
        }

        RefreshToken? existingRefreshToken = await _authRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive || existingRefreshToken.UserId != userId)
        {
            ApiResponse<RefreshTokenResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Invalid access token."],
                Message = "Refresh token failed"
            };

            return apiResponse;
        }

        User? user = await _authRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            ApiResponse<RefreshTokenResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["User not found."],
                Message = "Refresh token failed"
            };

            return apiResponse;
        }

        existingRefreshToken.RevokedAt = DateTime.UtcNow;
        RefreshToken newRefreshToken = _jwtService.GenerateRefreshToken(user);
        await _authRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        AuthResultDto authResult = CreateAuthResult(user, newRefreshToken.Token);

        return new ApiResponse<RefreshTokenResponse> { Success = true, Data = new RefreshTokenResponse { Auth = authResult }, Errors = [], Message = "Token refreshed successfully" };
    }

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        User? user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || user.IsActive is false)
        {
            return new ApiResponse<object>
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent.",
                Data = null,
                Errors = []
            };
        }

        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await _userRepository.SaveChangesAsync(cancellationToken);

        string encodedToken = Uri.EscapeDataString(token);
        string resetPasswordUrl = $"https://localhost:7033/user-panel/reset-password?token={encodedToken}";

        await _emailNotificationService.CreateAndSendPasswordResetEmailAsync(user.Id, resetPasswordUrl, cancellationToken);

        return new ApiResponse<object>
        {
            Success = true,
            Message = "If the email exists, a password reset link has been sent.",
            Data = null,
            Errors = []
        };
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Password could not be reset.",
                Data = null,
                Errors = ["New password and confirmation do not match."]
            };
        }

        User? user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Password could not be reset.",
                Data = null,
                Errors = ["Invalid or expired password reset token."]
            };
        }

        user.PasswordHash = _passwordService.HashPassword(user, request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEndAt = null;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Password reset successfully.",
            Data = null,
            Errors = []
        };
    }

    public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _changePasswordValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            ApiResponse<ChangePasswordResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                Message = "Validation failed"
            }; 

            return apiResponse;
        }

        User? user = await _authRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            ApiResponse<ChangePasswordResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["User not found."],
                Message = "Change password failed"
            }; 

            return apiResponse;
        }

        bool validPassword = _passwordService.VerifyPassword(user, command.CurrentPassword, user.PasswordHash);

        if (!validPassword)
        {
            ApiResponse<ChangePasswordResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["Current password is incorrect."],
                Message = "Change password failed"
            };

            return apiResponse;
        }

        user.PasswordHash = _passwordService.HashPassword(user, command.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;

        await _authRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ChangePasswordResponse> { Success = true, Data = new ChangePasswordResponse { PasswordChanged = true }, Errors = [], Message = "Password changed successfully" };
    }

    public async Task<ApiResponse<GetCurrentUserResponse>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default)
    {
        User? user = await _authRepository.GetUserByIdAsync(query.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            ApiResponse<GetCurrentUserResponse> apiResponse = new()
            {
                Success = false,
                Data = null,
                Errors = ["User not found."],
                Message = "Current user not found"
            };

            return apiResponse;
        }

        return new ApiResponse<GetCurrentUserResponse> { Success = true, Data = new GetCurrentUserResponse { User = _mapper.Map<AuthUserDto>(user) }, Errors = [], Message = "Current user retrieved successfully" };
    }

    public async Task<ApiResponse<LogoutResponse>> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await _authRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is not null && refreshToken.RevokedAt is null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _authRepository.SaveChangesAsync(cancellationToken);
        }

        return new ApiResponse<LogoutResponse> { Success = true, Data = new LogoutResponse { LoggedOut = true }, Errors = [], Message = "Logout successful" };
    }

    private AuthResultDto CreateAuthResult(User user, string refreshToken)
    {
        return new AuthResultDto { AccessToken = _jwtService.GenerateAccessToken(user), RefreshToken = refreshToken, ExpiresAt = _jwtService.GetAccessTokenExpiration(), User = _mapper.Map<AuthUserDto>(user) };
    }
}
