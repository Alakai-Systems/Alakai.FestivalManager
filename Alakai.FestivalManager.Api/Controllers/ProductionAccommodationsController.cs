namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionAccommodationsController : ControllerBase
{
    private readonly IProductionAccommodationService _accommodationService;
    private readonly IMapper _mapper;

    public ProductionAccommodationsController(IProductionAccommodationService accommodationService, IMapper mapper)
    {
        _accommodationService = accommodationService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionAccommodationRequest request, CancellationToken cancellationToken)
    {
        CreateProductionAccommodationCommand command = _mapper.Map<CreateProductionAccommodationCommand>(request);
        ApiResponse<CreateProductionAccommodationResponse> response = await _accommodationService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.ProductionAccommodation.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationsResponse> response = await _accommodationService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationByIdResponse> response = await _accommodationService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-zone/{zoneId:guid}")]
    public async Task<IActionResult> GetByZoneId(Guid zoneId, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionAccommodationsResponse> response = await _accommodationService.GetByZoneIdAsync(zoneId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionAccommodationRequest request, CancellationToken cancellationToken)
    {
        UpdateProductionAccommodationCommand command = _mapper.Map<UpdateProductionAccommodationCommand>(request);
        command.Id = id;
        ApiResponse<UpdateProductionAccommodationResponse> response = await _accommodationService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteProductionAccommodationResponse> response = await _accommodationService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}