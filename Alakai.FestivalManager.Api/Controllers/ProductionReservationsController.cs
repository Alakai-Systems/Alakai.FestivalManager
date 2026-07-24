namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionReservationsController : ControllerBase
{
    private readonly IProductionReservationService _reservationService;
    private readonly IMapper _mapper;

    public ProductionReservationsController(IProductionReservationService reservationService, IMapper mapper)
    {
        _reservationService = reservationService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        CreateReservationCommand command = _mapper.Map<CreateReservationCommand>(request);
        ApiResponse<CreateReservationResponse> response = await _reservationService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Reservation.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetReservationsResponse> response = await _reservationService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetReservationResponse> response = await _reservationService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-building/{buildingId:guid}")]
    public async Task<IActionResult> GetByBuildingId(Guid buildingId, CancellationToken cancellationToken)
    {
        ApiResponse<GetReservationsResponse> response = await _reservationService.GetByBuildingIdAsync(buildingId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReservationRequest request, CancellationToken cancellationToken)
    {
        UpdateReservationCommand command = _mapper.Map<UpdateReservationCommand>(request);
        command.ReservationId = id;
        ApiResponse<UpdateReservationResponse> response = await _reservationService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteReservationResponse> response = await _reservationService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}