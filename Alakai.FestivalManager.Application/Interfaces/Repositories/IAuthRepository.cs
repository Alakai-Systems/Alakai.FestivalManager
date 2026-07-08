namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

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
