namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/accommodation-zones")]
public class AccommodationZonesController : ControllerBase
{
    private readonly IAccommodationZoneService _zoneService;

    public AccommodationZonesController(IAccommodationZoneService zoneService)
    {
        _zoneService = zoneService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccommodationZoneCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _zoneService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccommodationZoneCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _zoneService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _zoneService.DeleteAsync(id, cancellationToken));
    }
}