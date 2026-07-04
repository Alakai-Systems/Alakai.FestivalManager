namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/bus-reservations")]
public class BusReservationsController : ControllerBase
{
    private readonly IBusReservationService _reservationService;

    public BusReservationsController(IBusReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<IActionResult> GetByRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.GetByRegistrationIdAsync(registrationId, cancellationToken));
    }

    [HttpGet("by-bus/{busId:guid}")]
    public async Task<IActionResult> GetByBus(Guid busId, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.GetByBusIdAsync(busId, cancellationToken));
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEdition(Guid editionId, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.GetByEditionIdAsync(editionId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusReservationCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.CreateAsync(command, cancellationToken));
    }

    [HttpPost("batch")]
    public async Task<IActionResult> CreateMany([FromBody] CreateBusReservationsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.CreateManyAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusReservationCommand command, [FromQuery] bool isAdmin, CancellationToken cancellationToken)
    {
        command.ReservationId = id;
        return Ok(await _reservationService.UpdateAsync(command, isAdmin, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid requestingRegistrationId, [FromQuery] bool isAdmin, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.DeleteAsync(id, requestingRegistrationId, isAdmin, cancellationToken));
    }
}