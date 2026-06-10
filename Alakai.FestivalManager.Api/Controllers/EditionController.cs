namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EditionsController : ControllerBase
{
    private readonly IEditionService _editionService;
    private readonly IMapper _mapper;

    public EditionsController(IEditionService editionService, IMapper mapper)
    {
        _editionService = editionService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEditionRequest request, CancellationToken cancellationToken)
    {
        CreateEditionCommand command = _mapper.Map<CreateEditionCommand>(request);

        ApiResponse<CreateEditionResponse> response = await _editionService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Edition.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionByIdResponse> response = await _editionService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpGet("by-festival/{festivalId:guid}")]
    public async Task<IActionResult> GetByFestivalId(Guid festivalId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionsResponse> response = await _editionService.GetByFestivalIdAsync(festivalId, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionsResponse> response = await _editionService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEditionRequest request, CancellationToken cancellationToken)
    {
        UpdateEditionCommand command = _mapper.Map<UpdateEditionCommand>(request);
        command.Id = id;

        ApiResponse<UpdateEditionResponse> response = await _editionService.UpdateAsync(command, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEditionResponse> response = await _editionService.DeleteAsync(id, cancellationToken);

        return Ok(response);
    }
}