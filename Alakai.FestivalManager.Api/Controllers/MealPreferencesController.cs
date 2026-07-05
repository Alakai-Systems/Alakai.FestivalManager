namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/meal-preferences")]
public class MealPreferencesController : ControllerBase
{
    private readonly IMealPreferenceService _mealPreferenceService;

    public MealPreferencesController(IMealPreferenceService mealPreferenceService)
    {
        _mealPreferenceService = mealPreferenceService;
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<IActionResult> GetByRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _mealPreferenceService.GetByRegistrationIdAsync(registrationId, cancellationToken));
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEdition(Guid editionId, CancellationToken cancellationToken)
    {
        return Ok(await _mealPreferenceService.GetByEditionIdAsync(editionId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveMealPreferenceCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mealPreferenceService.SaveAsync(command, cancellationToken));
    }
}