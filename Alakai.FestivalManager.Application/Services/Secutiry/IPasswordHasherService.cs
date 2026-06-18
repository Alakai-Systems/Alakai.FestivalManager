namespace Alakai.FestivalManager.Application.Services.Security;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
}