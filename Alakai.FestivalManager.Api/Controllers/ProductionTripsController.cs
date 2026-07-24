namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionTripsController : ControllerBase
{
    private readonly IProductionTripService _tripService;
    private readonly IMapper _mapper;

    public ProductionTripsController(IProductionTripService tripService, IMapper mapper)
    {
        _tripService = tripService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTripRequest request, CancellationToken cancellationToken)
    {
        CreateTripCommand command = _mapper.Map<CreateTripCommand>(request);
        ApiResponse<CreateTripResponse> response = await _tripService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Trip.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetTripResponse> response = await _tripService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetTripsResponse> response = await _tripService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTripRequest request, CancellationToken cancellationToken)
    {
        UpdateTripCommand command = _mapper.Map<UpdateTripCommand>(request);
        command.Id = id;
        ApiResponse<UpdateTripResponse> response = await _tripService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteTripResponse> response = await _tripService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}