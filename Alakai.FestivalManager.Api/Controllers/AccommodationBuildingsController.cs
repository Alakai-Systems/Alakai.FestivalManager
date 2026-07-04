namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/accommodation-buildings")]
public class AccommodationBuildingsController : ControllerBase
{
    private readonly IAccommodationBuildingService _buildingService;

    public AccommodationBuildingsController(IAccommodationBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        return Ok(await _buildingService.GetAllAsync(editionId, cancellationToken));
    }

    [HttpGet("available-for-registration/{registrationId:guid}")]
    public async Task<IActionResult> GetAvailableForRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        return Ok(await _buildingService.GetAvailableForRegistrationAsync(registrationId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _buildingService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccommodationBuildingCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _buildingService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccommodationBuildingCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _buildingService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _buildingService.DeleteAsync(id, cancellationToken));
    }
}