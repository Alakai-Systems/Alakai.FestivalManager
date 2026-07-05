namespace Alakai.FestivalManager.Admin.Contracts.Meals.Requests;

public class SaveMealPreferenceRequest
{
    public Guid RegistrationId { get; set; }
    public int MenuType { get; set; } = 1;
    public bool IsCeliacOrGlutenIntolerant { get; set; }
    public string? AllergiesNotes { get; set; }
}