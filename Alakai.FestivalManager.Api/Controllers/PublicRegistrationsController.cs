namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/registrations")]
public class PublicRegistrationsController : ControllerBase
{
    private readonly IPublicRegistrationService _publicRegistrationService;

    public PublicRegistrationsController(IPublicRegistrationService publicRegistrationService)
    {
        _publicRegistrationService = publicRegistrationService;
    }

    [HttpGet("availability/{editionId:guid}")]
    public async Task<IActionResult> GetAvailability(Guid editionId, CancellationToken cancellationToken)
    {
        return Ok(await _publicRegistrationService.GetAvailabilityAsync(editionId, cancellationToken));
    }

    [HttpGet("discount-check")]
    public async Task<IActionResult> CheckDiscount([FromQuery] Guid editionId, [FromQuery] string code, [FromQuery] decimal basePrice, CancellationToken cancellationToken)
    {
        return Ok(await _publicRegistrationService.CheckDiscountCodeAsync(editionId, code ?? string.Empty, basePrice, cancellationToken));
    }

    [HttpGet("partner-lookup")]
    public async Task<IActionResult> LookupPartner([FromQuery] Guid editionId, [FromQuery] string email, CancellationToken cancellationToken)
    {
        return Ok(await _publicRegistrationService.LookupPartnerAsync(editionId, email ?? string.Empty, cancellationToken));
    }
}
