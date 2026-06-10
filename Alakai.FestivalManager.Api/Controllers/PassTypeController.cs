namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PassTypesController : ControllerBase
{
    private readonly IPassTypeService _passTypeService;
    private readonly IMapper _mapper;

    public PassTypesController(IPassTypeService passTypeService, IMapper mapper)
    {
        _passTypeService = passTypeService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePassTypeRequest request, CancellationToken cancellationToken)
    {
        CreatePassTypeCommand command = _mapper.Map<CreatePassTypeCommand>(request);
        ApiResponse<CreatePassTypeResponse> response = await _passTypeService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.PassType.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetPassTypeByIdResponse> response = await _passTypeService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetPassTypesResponse> response = await _passTypeService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetPassTypesResponse> response = await _passTypeService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePassTypeRequest request, CancellationToken cancellationToken)
    {
        UpdatePassTypeCommand command = _mapper.Map<UpdatePassTypeCommand>(request);
        command.Id = id;
        ApiResponse<UpdatePassTypeResponse> response = await _passTypeService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeletePassTypeResponse> response = await _passTypeService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
