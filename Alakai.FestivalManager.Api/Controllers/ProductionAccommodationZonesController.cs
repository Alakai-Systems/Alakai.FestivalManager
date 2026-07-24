namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionAccommodationZonesController : ControllerBase
{
    private readonly IProductionAccommodationZoneService _zoneService;
    private readonly IMapper _mapper;

    public ProductionAccommodationZonesController(IProductionAccommodationZoneService zoneService, IMapper mapper)
    {
        _zoneService = zoneService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionAccommodationZoneRequest request, CancellationToken cancellationToken)
    {
        CreateProductionAccommodationZoneCommand command = _mapper.Map<CreateProductionAccommodationZoneCommand>(request);
        ApiResponse<CreateProductionAccommodationZoneResponse> response = await _zoneService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.ProductionAccommodationZone.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationZonesResponse> response = await _zoneService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationZoneByIdResponse> response = await _zoneService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-building/{buildingId:guid}")]
    public async Task<IActionResult> GetByBuildingId(Guid buildingId, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationZonesResponse> response = await _zoneService.GetByBuildingIdAsync(buildingId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionAccommodationZoneRequest request, CancellationToken cancellationToken)
    {
        UpdateProductionAccommodationZoneCommand command = _mapper.Map<UpdateProductionAccommodationZoneCommand>(request);
        command.Id = id;
        ApiResponse<UpdateProductionAccommodationZoneResponse> response = await _zoneService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteProductionAccommodationZoneResponse> response = await _zoneService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}