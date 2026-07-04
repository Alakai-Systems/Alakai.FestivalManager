using FestivalDto = Alakai.FestivalManager.Admin.Contracts.Festivals.DTOs.FestivalDto;

namespace Alakai.FestivalManager.Admin.Services;

public class ActiveFestivalState
{
    public FestivalDto? Active { get; private set; }
    public IReadOnlyList<FestivalDto> AllFestivals { get; private set; } = [];
    public bool IsInitialized { get; private set; }

    public event Action? OnChange;

    public void Initialize(IReadOnlyList<FestivalDto> festivals, Guid? preferredFestivalId)
    {
        AllFestivals = festivals;

        FestivalDto? preferred = preferredFestivalId.HasValue
            ? festivals.FirstOrDefault(f => f.Id == preferredFestivalId.Value)
            : null;

        Active = preferred ?? festivals.FirstOrDefault(f => f.IsActive) ?? festivals.FirstOrDefault();
        IsInitialized = true;
        OnChange?.Invoke();
    }

    public void SetActive(FestivalDto festival)
    {
        Active = festival;
        OnChange?.Invoke();
    }

    public bool HasModule(FestivalModule module)
    {
        if (Active is null)
        {
            return false;
        }

        return (Active.EnabledModules & (int)module) != 0;
    }
}