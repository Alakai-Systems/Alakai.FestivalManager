namespace Alakai.FestivalManager.Admin.Contracts.Meals.DTOs;

public class MealPreferenceDto
{
    public Guid? Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string? RegistrationName { get; set; }
    public string? RegistrationEmail { get; set; }
    public bool HasPreference { get; set; }
    public int MenuType { get; set; }
    public bool IsCeliacOrGlutenIntolerant { get; set; }
    public string? AllergiesNotes { get; set; }
}