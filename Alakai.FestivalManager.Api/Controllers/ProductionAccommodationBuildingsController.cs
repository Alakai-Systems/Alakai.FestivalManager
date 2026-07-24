namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionAccommodationBuildingsController : ControllerBase
{
    private readonly IProductionAccommodationBuildingService _buildingService;
    private readonly IMapper _mapper;

    public ProductionAccommodationBuildingsController(IProductionAccommodationBuildingService buildingService, IMapper mapper)
    {
        _buildingService = buildingService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionAccommodationBuildingRequest request, CancellationToken cancellationToken)
    {
        CreateBuildingCommand command = _mapper.Map<CreateBuildingCommand>(request);
        ApiResponse<CreateProductionAccommodationBuildingResponse> response = await _buildingService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.ProductionAccommodationBuilding.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationBuildingByIdResponse> response = await _buildingService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationBuildingsResponse> response = await _buildingService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationBuildingsResponse> response = await _buildingService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionAccommodationBuildingRequest request, CancellationToken cancellationToken)
    {
        UpdateBuildingCommand command = _mapper.Map<UpdateBuildingCommand>(request);
        command.Id = id;
        ApiResponse<UpdateProductionAccommodationBuildingResponse> response = await _buildingService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteProductionAccommodationBuildingResponse> response = await _buildingService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}