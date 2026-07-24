namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController : ControllerBase
{
    private readonly IBusService _busService;

    public BusesController(IBusService busService)
    {
        _busService = busService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        return Ok(await _busService.GetAllAsync(editionId, cancellationToken));
    }

    [HttpGet("available-for-registration/{registrationId:guid}")]
    public async Task<IActionResult> GetAvailableForRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _busService.GetAvailableForRegistrationAsync(registrationId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _busService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _busService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _busService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _busService.DeleteAsync(id, cancellationToken));
    }
}