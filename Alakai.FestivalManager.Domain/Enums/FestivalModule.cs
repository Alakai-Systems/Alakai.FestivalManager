namespace Alakai.FestivalManager.Domain.Enums;

[Flags]
public enum FestivalModule
{
    None = 0,
    Competitions = 1,
    Accommodation = 2,
    Transport = 4,
    Meals = 8
}