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
