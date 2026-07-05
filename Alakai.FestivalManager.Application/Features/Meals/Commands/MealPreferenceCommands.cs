namespace Alakai.FestivalManager.Application.Features.Meals.Commands;

public class SaveMealPreferenceCommand
{
    public Guid RegistrationId { get; set; }
    public MenuType MenuType { get; set; }
    public bool IsCeliacOrGlutenIntolerant { get; set; }
    public string? AllergiesNotes { get; set; }
}