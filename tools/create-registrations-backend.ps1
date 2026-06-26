param(
    [string]$SolutionRoot = "."
)

$ErrorActionPreference = "Stop"

function Write-File {
    param([string]$Path, [string]$Content)
    $directory = Split-Path $Path -Parent
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    Set-Content -Path $Path -Value $Content -Encoding UTF8
    Write-Host "Written: $Path"
}

function Add-LineIfMissing {
    param([string]$Path, [string]$Line)
    if (-not (Test-Path $Path)) { Write-Host "Skipped. File not found: $Path"; return }
    $content = Get-Content -Path $Path -Raw
    if ($content.Contains($Line)) { Write-Host "Already present: $Line"; return }
    Add-Content -Path $Path -Value $Line -Encoding UTF8
    Write-Host "Added: $Line"
}

function Patch-JsonJwtIfMissing {
    param([string]$Path)
    if (-not (Test-Path $Path)) { Write-Host "Skipped appsettings patch. File not found: $Path"; return }
    $content = Get-Content -Path $Path -Raw
    if ($content.Contains('"Jwt"')) { Write-Host "Jwt settings already present."; return }
    $trimmed = $content.TrimEnd()
    $lastBraceIndex = $trimmed.LastIndexOf("}")
    if ($lastBraceIndex -lt 0) { Write-Host "Skipped appsettings patch. Invalid JSON shape."; return }
    $beforeLastBrace = $trimmed.Substring(0, $lastBraceIndex).TrimEnd()
    $afterLastBrace = $trimmed.Substring($lastBraceIndex)
    if ($beforeLastBrace.EndsWith("{")) {
        $jwtBlock = "`r`n  `"Jwt`": {`r`n    `"Issuer`": `"AlakaiFestivalManager`",`r`n    `"Audience`": `"AlakaiFestivalManager`",`r`n    `"SecretKey`": `"THIS_IS_A_DEVELOPMENT_SECRET_KEY_CHANGE_IT_IN_PRODUCTION_2026`",`r`n    `"ExpirationMinutes`": 120,`r`n    `"RefreshTokenExpirationDays`": 30`r`n  }`r`n"
    }
    else {
        $jwtBlock = ",`r`n  `"Jwt`": {`r`n    `"Issuer`": `"AlakaiFestivalManager`",`r`n    `"Audience`": `"AlakaiFestivalManager`",`r`n    `"SecretKey`": `"THIS_IS_A_DEVELOPMENT_SECRET_KEY_CHANGE_IT_IN_PRODUCTION_2026`",`r`n    `"ExpirationMinutes`": 120,`r`n    `"RefreshTokenExpirationDays`": 30`r`n  }`r`n"
    }
    Set-Content -Path $Path -Value ($beforeLastBrace + $jwtBlock + $afterLastBrace) -Encoding UTF8
    Write-Host "Patched Jwt settings in appsettings.json"
}

$root = Resolve-Path $SolutionRoot
$applicationRoot = Join-Path $root "Alakai.FestivalManager.Application"
$infrastructureRoot = Join-Path $root "Alakai.FestivalManager.Infrastructure"
$apiRoot = Join-Path $root "Alakai.FestivalManager.Api"

if (-not (Test-Path $applicationRoot)) { throw "Application project not found." }
if (-not (Test-Path $infrastructureRoot)) { throw "Infrastructure project not found." }
if (-not (Test-Path $apiRoot)) { throw "Api project not found." }

Write-Host "Adding NuGet packages..."
dotnet add "$apiRoot\Alakai.FestivalManager.Api.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add "$applicationRoot\Alakai.FestivalManager.Application.csproj" package Microsoft.Extensions.Identity.Core
dotnet add "$applicationRoot\Alakai.FestivalManager.Application.csproj" package System.IdentityModel.Tokens.Jwt
dotnet add "$applicationRoot\Alakai.FestivalManager.Application.csproj" package Microsoft.Extensions.Options.ConfigurationExtensions

Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ChangePassword;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ForgotPassword;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.Login;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.Logout;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.RefreshToken;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ResetPassword;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.DTOs;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Settings;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Queries.GetCurrentUser;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Services;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Microsoft.AspNetCore.Identity;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Microsoft.Extensions.Options;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using Microsoft.IdentityModel.Tokens;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using System.IdentityModel.Tokens.Jwt;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using System.Security.Claims;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using System.Security.Cryptography;"
Add-LineIfMissing "$applicationRoot\GlobalUsings.cs" "global using System.Text;"

Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ChangePassword;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ForgotPassword;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.Login;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.Logout;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.RefreshToken;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Commands.ResetPassword;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Contracts.Settings;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Queries.GetCurrentUser;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.Auth.Services;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Microsoft.AspNetCore.Authentication.JwtBearer;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Microsoft.AspNetCore.Authorization;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using Microsoft.IdentityModel.Tokens;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using System.Security.Claims;"
Add-LineIfMissing "$apiRoot\GlobalUsings.cs" "global using System.Text;"

Write-File "$applicationRoot\Features\Auth\Contracts\Settings\JwtSettings.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Settings;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\DTOs\AuthUserDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.DTOs;

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool MustChangePassword { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\DTOs\AuthResultDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.DTOs;

public class AuthResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = default!;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Requests\LoginRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Requests\RefreshTokenRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Requests\ForgotPasswordRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Requests\ResetPasswordRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Requests\ChangePasswordRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\Login\LoginCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.Login;

public class LoginCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\RefreshToken\RefreshTokenCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\ForgotPassword\ForgotPasswordCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand
{
    public string Email { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\ResetPassword\ResetPasswordCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\ChangePassword\ChangePasswordCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Commands\Logout\LogoutCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Commands.Logout;

public class LogoutCommand
{
    public string RefreshToken { get; set; } = string.Empty;
}
'@

Write-File "$applicationRoot\Features\Auth\Queries\GetCurrentUser\GetCurrentUserQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery
{
    public Guid UserId { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\LoginResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class LoginResponse
{
    public AuthResultDto Auth { get; set; } = default!;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\RefreshTokenResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class RefreshTokenResponse
{
    public AuthResultDto Auth { get; set; } = default!;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\ForgotPasswordResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class ForgotPasswordResponse
{
    public bool Sent { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\ResetPasswordResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class ResetPasswordResponse
{
    public bool PasswordReset { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\ChangePasswordResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class ChangePasswordResponse
{
    public bool PasswordChanged { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\GetCurrentUserResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class GetCurrentUserResponse
{
    public AuthUserDto User { get; set; } = default!;
}
'@

Write-File "$applicationRoot\Features\Auth\Contracts\Responses\LogoutResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class LogoutResponse
{
    public bool LoggedOut { get; set; }
}
'@

Write-File "$applicationRoot\Features\Auth\Validators\LoginCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(l => l.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(l => l.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Validators\RefreshTokenCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(r => r.AccessToken).NotEmpty();
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Validators\ForgotPasswordCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(f => f.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Validators\ResetPasswordCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(r => r.Token).NotEmpty();
        RuleFor(r => r.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Validators\ChangePasswordCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CurrentPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Mapping\AuthMappingProfile.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<LoginRequest, LoginCommand>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();
        CreateMap<ForgotPasswordRequest, ForgotPasswordCommand>();
        CreateMap<ResetPasswordRequest, ResetPasswordCommand>();
        CreateMap<ChangePasswordRequest, ChangePasswordCommand>();
        CreateMap<User, AuthUserDto>();
    }
}
'@

Write-File "$applicationRoot\Contracts\Repositories\IAuthRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddPasswordResetTokenAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

Write-File "$applicationRoot\Features\Auth\Services\IPasswordService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}
'@

Write-File "$applicationRoot\Features\Auth\Services\PasswordService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password, string passwordHash)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Services\IJwtService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiration();
}
'@

Write-File "$applicationRoot\Features\Auth\Services\JwtService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiresAt = GetAccessTokenExpiration();

        JwtSecurityToken token = new(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience, claims: claims, expires: expiresAt, signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        string token = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        };

        JwtSecurityTokenHandler handler = new();

        return handler.ValidateToken(token, parameters, out SecurityToken _);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
    }
}
'@

Write-File "$applicationRoot\Features\Auth\Services\IAuthService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCurrentUserResponse>> GetCurrentUserAsync(GetCurrentUserQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<LogoutResponse>> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default);
}
'@

Write-File "$applicationRoot\Features\Auth\Services\AuthService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginCommand> _loginValidator;
    private readonly IValidator<RefreshTokenCommand> _refreshTokenValidator;
    private readonly IValidator<ForgotPasswordCommand> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordCommand> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordCommand> _changePasswordValidator;

    public AuthService(IAuthRepository authRepository, IPasswordService passwordService, IJwtService jwtService, IMapper mapper, IValidator<LoginCommand> loginValidator, IValidator<RefreshTokenCommand> refreshTokenValidator, IValidator<ForgotPasswordCommand> forgotPasswordValidator, IValidator<ResetPasswordCommand> resetPasswordValidator, IValidator<ChangePasswordCommand> changePasswordValidator)
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
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _loginValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<LoginResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        string normalizedEmail = command.Email.Trim().ToLowerInvariant();
        User? user = await _authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<LoginResponse>.Failure(["Invalid email or password."], "Login failed");
        }

        if (user.IsLocked && user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
        {
            return ApiResponse<LoginResponse>.Failure(["User is temporarily locked."], "Login failed");
        }

        bool validPassword = _passwordService.VerifyPassword(user, command.Password, user.PasswordHash);

        if (!validPassword)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
            }

            await _authRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<LoginResponse>.Failure(["Invalid email or password."], "Login failed");
        }

        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEndAt = null;
        user.LastLoginAt = DateTime.UtcNow;

        RefreshToken refreshToken = _jwtService.GenerateRefreshToken(user);
        await _authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        AuthResultDto authResult = CreateAuthResult(user, refreshToken.Token);

        return new ApiResponse<LoginResponse> { Success = true, Data = new LoginResponse { Auth = authResult }, Errors = [], Message = "Login successful" };
    }

    public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _refreshTokenValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<RefreshTokenResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromExpiredToken(command.AccessToken);

        if (principal is null)
        {
            return ApiResponse<RefreshTokenResponse>.Failure(["Invalid access token."], "Refresh token failed");
        }

        string? userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return ApiResponse<RefreshTokenResponse>.Failure(["Invalid access token."], "Refresh token failed");
        }

        RefreshToken? existingRefreshToken = await _authRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive || existingRefreshToken.UserId != userId)
        {
            return ApiResponse<RefreshTokenResponse>.Failure(["Invalid refresh token."], "Refresh token failed");
        }

        User? user = await _authRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<RefreshTokenResponse>.Failure(["User not found."], "Refresh token failed");
        }

        existingRefreshToken.RevokedAt = DateTime.UtcNow;
        RefreshToken newRefreshToken = _jwtService.GenerateRefreshToken(user);
        await _authRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        AuthResultDto authResult = CreateAuthResult(user, newRefreshToken.Token);

        return new ApiResponse<RefreshTokenResponse> { Success = true, Data = new RefreshTokenResponse { Auth = authResult }, Errors = [], Message = "Token refreshed successfully" };
    }

    public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _forgotPasswordValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<ForgotPasswordResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        string normalizedEmail = command.Email.Trim().ToLowerInvariant();
        User? user = await _authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is not null && user.IsActive)
        {
            PasswordResetToken passwordResetToken = new()
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };

            await _authRepository.AddPasswordResetTokenAsync(passwordResetToken, cancellationToken);
            await _authRepository.SaveChangesAsync(cancellationToken);
        }

        return new ApiResponse<ForgotPasswordResponse> { Success = true, Data = new ForgotPasswordResponse { Sent = true }, Errors = [], Message = "If the email exists, a reset link has been generated" };
    }

    public async Task<ApiResponse<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _resetPasswordValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<ResetPasswordResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        PasswordResetToken? passwordResetToken = await _authRepository.GetPasswordResetTokenAsync(command.Token, cancellationToken);

        if (passwordResetToken is null || passwordResetToken.IsUsed || passwordResetToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ApiResponse<ResetPasswordResponse>.Failure(["Invalid or expired reset token."], "Reset password failed");
        }

        User? user = await _authRepository.GetUserByIdAsync(passwordResetToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<ResetPasswordResponse>.Failure(["User not found."], "Reset password failed");
        }

        user.PasswordHash = _passwordService.HashPassword(user, command.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEndAt = null;
        passwordResetToken.UsedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ResetPasswordResponse> { Success = true, Data = new ResetPasswordResponse { PasswordReset = true }, Errors = [], Message = "Password reset successfully" };
    }

    public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _changePasswordValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<ChangePasswordResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        User? user = await _authRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<ChangePasswordResponse>.Failure(["User not found."], "Change password failed");
        }

        bool validPassword = _passwordService.VerifyPassword(user, command.CurrentPassword, user.PasswordHash);

        if (!validPassword)
        {
            return ApiResponse<ChangePasswordResponse>.Failure(["Current password is incorrect."], "Change password failed");
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
            return ApiResponse<GetCurrentUserResponse>.Failure(["User not found."], "Current user not found");
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
'@

Write-File "$infrastructureRoot\Repositories\AuthRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly FestivalManagerDbContext _context;

    public AuthRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
    }

    public async Task AddPasswordResetTokenAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(passwordResetToken, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens.FirstOrDefaultAsync(p => p.Token == token, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

Write-File "$apiRoot\Controllers\AuthController.cs" @'
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        LoginCommand command = _mapper.Map<LoginCommand>(request);
        ApiResponse<LoginResponse> response = await _authService.LoginAsync(command, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        RefreshTokenCommand command = _mapper.Map<RefreshTokenCommand>(request);
        ApiResponse<RefreshTokenResponse> response = await _authService.RefreshTokenAsync(command, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        ForgotPasswordCommand command = _mapper.Map<ForgotPasswordCommand>(request);
        ApiResponse<ForgotPasswordResponse> response = await _authService.ForgotPasswordAsync(command, cancellationToken);

        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ResetPasswordCommand command = _mapper.Map<ResetPasswordCommand>(request);
        ApiResponse<ResetPasswordResponse> response = await _authService.ResetPasswordAsync(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ChangePasswordCommand command = _mapper.Map<ChangePasswordCommand>(request);
        command.UserId = userId;

        ApiResponse<ChangePasswordResponse> response = await _authService.ChangePasswordAsync(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<GetCurrentUserResponse>>> Me(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetCurrentUserResponse> response = await _authService.GetCurrentUserAsync(new GetCurrentUserQuery { UserId = userId }, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<LogoutResponse> response = await _authService.LogoutAsync(command, cancellationToken);

        return Ok(response);
    }
}
'@

Write-File "$root\AUTH_DI_TO_ADD.txt" @'
ApplicationDependencyInjectionExtension.cs:

//Auth
services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<IJwtService, JwtService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
services.AddScoped<IValidator<ForgotPasswordCommand>, ForgotPasswordCommandValidator>();
services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();
services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();

InfrastructureDependencyInjectionExtension.cs:

//Auth
services.AddScoped<IAuthRepository, AuthRepository>();

Program.cs in Api, before builder.Services.AddControllers():

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        JwtSettings jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

Program.cs in Api, before app.MapControllers():

app.UseAuthentication();
app.UseAuthorization();
'@

Patch-JsonJwtIfMissing "$apiRoot\appsettings.json"

Write-Host ""
Write-Host "Auth backend files generated."
Write-Host "Review AUTH_DI_TO_ADD.txt and add DI/JWT middleware manually."
Write-Host "Run dotnet build after adding DI registrations."

Alakai.FestivalManager.Admin
│
├── Components
│   ├── Layout
│   │   ├── MainLayout.razor
│   │   ├── AuthLayout.razor
│   │   └── PortalLayout.razor
│   │
│   └── Pages
│       ├── Auth
│       │   ├── Login.razor
│       │   ├── ForgotPassword.razor
│       │   └── ResetPassword.razor
│       │
│       └── Portal
│           └── Dashboard.razor
│
├── Services
│   └── Auth
│       ├── IAuthApiClient.cs
│       ├── AuthApiClient.cs
│       ├── ITokenStorageService.cs
│       ├── TokenStorageService.cs
│       ├── PortalAuthenticationStateProvider.cs
│       └── CurrentUserService.cs
│
├── Models
│   └── Auth
│       ├── LoginRequest.cs
│       ├── LoginResponse.cs
│       ├── ForgotPasswordRequest.cs
│       ├── ResetPasswordRequest.cs
│       └── AuthUserDto.cs


eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2YzdiNWI4Ny02M2NmLTQ0MjQtOGI2Ni1jMDJjOTkxMGU4MDIiLCJlbWFpbCI6InJvc2VyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiNmM3YjViODctNjNjZi00NDI0LThiNjYtYzAyYzk5MTBlODAyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoicm9zZXJAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJSb3NlciBSb3MiLCJleHAiOjE3ODIyMTk3ODksImlzcyI6IkFsYWthaUZlc3RpdmFsTWFuYWdlciIsImF1ZCI6IkFsYWthaUZlc3RpdmFsTWFuYWdlciJ9.sN6qAG0S3-MTGprGfx_bP5kNiSj8XBcK7t6yWfDHjZA

{
  "editionId": "6F705DAC-B15F-4E3D-8691-9099E57503DB",
  "passTypeId": "EE3F5EF2-DED4-452C-B067-990FA9569148",
  "levelId": "BDAE50D5-1690-4257-A15F-3660ED8F17DA",
  "firstName": "Roser",
  "lastName": "Ros",
  "email": "roser@gmail.com",
  "phone": "698521458",
  "country": "Spain",
  "city": "Barcelona",
  "password": "Password123!",
  "documentNumber": "254875235I",
  "documentCountry": "Spain",
  "danceRole": 1,
  "partnerEmail": "mago@gmail.com",
  "basePrice": 470,
  "discountAmount": 0,
  "finalPrice": 0,
  "discountCodeValue": "SWIM",
  "notes": "string",
  "internalNotes": "string"
}