namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/accommodations")]
public class AccommodationsController : ControllerBase
{
    private readonly IAccommodationService _accommodationService;

    public AccommodationsController(IAccommodationService accommodationService)
    {
        _accommodationService = accommodationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccommodationCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _accommodationService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccommodationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _accommodationService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _accommodationService.DeleteAsync(id, cancellationToken));
    }
}