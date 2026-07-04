namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationFestivalInfoController : ControllerBase
{
    private readonly IRegistrationFestivalInfoService _service;

    public RegistrationFestivalInfoController(IRegistrationFestivalInfoService service)
    {
        _service = service;
    }

    [HttpGet("{registrationId:guid}/festival-info")]
    public async Task<IActionResult> GetFestivalInfo(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetForRegistrationAsync(registrationId, cancellationToken));
    }
}