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
