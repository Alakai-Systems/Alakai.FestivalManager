namespace Alakai.FestivalManager.Admin.Services.Auth;

public class TokenStorageService : ITokenStorageService
{
    private readonly ProtectedLocalStorage _localStorage;

    public TokenStorageService(
        ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await _localStorage.SetAsync("access_token", token);
        }
        catch (InvalidOperationException)
        {

        }
    }
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<string> result = await _localStorage.GetAsync<string>("access_token");

            return result.Success
                ? result.Value
                : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            await _localStorage.DeleteAsync("access_token");
        }
        catch (InvalidOperationException)
        {
        }
    }
}