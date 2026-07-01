namespace Alakai.FestivalManager.Admin.Services;

public class UserProfileState
{
    public string? PhotoUrl { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action? OnChange;

    public void SetPhoto(string? photoUrl)
    {
        PhotoUrl = photoUrl;
        IsLoaded = true;
        OnChange?.Invoke();
    }
}