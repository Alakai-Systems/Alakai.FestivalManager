namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class ProductionSuppliersController : ControllerBase
{
    private readonly IProductionSupplierService _productionSupplierService;
    private readonly IMapper _mapper;

    public ProductionSuppliersController(IProductionSupplierService productionSupplierService, IMapper mapper)
    {
        _productionSupplierService = productionSupplierService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionSupplierRequest request, CancellationToken cancellationToken)
    {
        CreateProductionSupplierCommand command = _mapper.Map<CreateProductionSupplierCommand>(request);
        ApiResponse<CreateProductionSupplierResponse> response = await _productionSupplierService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.ProductionSupplier.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionSupplierByIdResponse> response = await _productionSupplierService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionSuppliersResponse> response = await _productionSupplierService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetProductionSuppliersResponse> response = await _productionSupplierService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionSupplierRequest request, CancellationToken cancellationToken)
    {
        UpdateProductionSupplierCommand command = _mapper.Map<UpdateProductionSupplierCommand>(request);
        command.Id = id;
        ApiResponse<UpdateProductionSupplierResponse> response = await _productionSupplierService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteProductionSupplierResponse> response = await _productionSupplierService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}