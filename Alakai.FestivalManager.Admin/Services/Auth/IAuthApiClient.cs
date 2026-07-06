namespace Alakai.FestivalManager.Admin.Services.Auth;

public interface IAuthApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> ExternalLoginAsync(ExternalLoginRequest request);
    Task<string?> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<string?> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, string? accessToken = null);
    Task<AuthResultDto?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
}