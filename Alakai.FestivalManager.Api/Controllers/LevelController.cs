
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LevelsController : ControllerBase
{
    private readonly ILevelService _levelService;
    private readonly IMapper _mapper;

    public LevelsController(ILevelService levelService, IMapper mapper)
    {
        _levelService = levelService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLevelRequest request, CancellationToken cancellationToken)
    {
        CreateLevelCommand command = _mapper.Map<CreateLevelCommand>(request);
        ApiResponse<CreateLevelResponse> response = await _levelService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Level.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetLevelByIdResponse> response = await _levelService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetLevelsResponse> response = await _levelService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-pass-type/{passTypeId:guid}")]
    public async Task<IActionResult> GetByPassTypeId(Guid passTypeId, CancellationToken cancellationToken)
    {
        ApiResponse<GetLevelsResponse> response = await _levelService.GetByPassTypeIdAsync(passTypeId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLevelRequest request, CancellationToken cancellationToken)
    {
        UpdateLevelCommand command = _mapper.Map<UpdateLevelCommand>(request);
        command.Id = id;
        ApiResponse<UpdateLevelResponse> response = await _levelService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteLevelResponse> response = await _levelService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
