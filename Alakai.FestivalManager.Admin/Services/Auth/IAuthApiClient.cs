namespace Alakai.FestivalManager.Admin.Services.Auth;

public interface IAuthApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string password);
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}
