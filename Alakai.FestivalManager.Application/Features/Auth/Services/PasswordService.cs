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
