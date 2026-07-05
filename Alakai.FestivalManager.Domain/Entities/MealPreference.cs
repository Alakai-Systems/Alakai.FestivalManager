namespace Alakai.FestivalManager.Domain.Entities;

public class MealPreference : BaseEntity
{
    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public MenuType MenuType { get; set; }
    public bool IsCeliacOrGlutenIntolerant { get; set; }
    public string? AllergiesNotes { get; set; }
}