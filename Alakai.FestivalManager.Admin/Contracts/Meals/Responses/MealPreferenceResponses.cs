using Alakai.FestivalManager.Admin.Contracts.Meals.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Meals.Responses;

public class GetMealPreferenceResponse
{
    public MealPreferenceDto? Preference { get; set; }
}

public class GetMealPreferencesResponse
{
    public List<MealPreferenceDto> Preferences { get; set; } = [];
}

public class SaveMealPreferenceResponse
{
    public MealPreferenceDto Preference { get; set; } = default!;
}