namespace Alakai.FestivalManager.Application.Features.Meals.Contracts.DTOs;

public class MealPreferenceDto
{
    public Guid? Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string? RegistrationName { get; set; }
    public string? RegistrationEmail { get; set; }
    public bool HasPreference { get; set; }
    public MenuType MenuType { get; set; }
    public bool IsCeliacOrGlutenIntolerant { get; set; }
    public string? AllergiesNotes { get; set; }
}