namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionPeopleController : ControllerBase
{
    private readonly IProductionPersonService _productionPersonService;
    private readonly IMapper _mapper;

    public ProductionPeopleController(IProductionPersonService productionPersonService, IMapper mapper)
    {
        _productionPersonService = productionPersonService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionPersonRequest request, CancellationToken cancellationToken)
    {
        CreateProductionPersonCommand command = _mapper.Map<CreateProductionPersonCommand>(request);
        ApiResponse<CreateProductionPersonResponse> response = await _productionPersonService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.ProductionPerson.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionPersonByIdResponse> response = await _productionPersonService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionPeopleResponse> response = await _productionPersonService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionPeopleResponse> response = await _productionPersonService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionPersonRequest request, CancellationToken cancellationToken)
    {
        UpdateProductionPersonCommand command = _mapper.Map<UpdateProductionPersonCommand>(request);
        command.Id = id;
        ApiResponse<UpdateProductionPersonResponse> response = await _productionPersonService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteProductionPersonResponse> response = await _productionPersonService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}