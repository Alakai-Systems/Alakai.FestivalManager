using Alakai.FestivalManager.Application.Features.Meals.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Meals.Contracts.Responses;

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