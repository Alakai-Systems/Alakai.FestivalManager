namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}
