namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/accommodation-reservations")]
public class AccommodationReservationsController : ControllerBase
{
    private readonly IAccommodationReservationService _reservationService;

    public AccommodationReservationsController(IAccommodationReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet("by-building/{buildingId:guid}")]
    public async Task<IActionResult> GetByBuilding(Guid buildingId, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.GetByBuildingIdAsync(buildingId, cancellationToken));
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<IActionResult> GetByResponsibleRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.GetByResponsibleRegistrationIdAsync(registrationId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _reservationService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccommodationReservationCommand command, [FromQuery] bool isAdmin, CancellationToken cancellationToken)
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