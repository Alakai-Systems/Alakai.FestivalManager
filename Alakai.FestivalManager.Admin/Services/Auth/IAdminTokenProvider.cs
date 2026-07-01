namespace Alakai.FestivalManager.Admin.Services.Auth;

public interface IAdminTokenProvider
{
    Task<string?> GetValidAccessTokenAsync();
}