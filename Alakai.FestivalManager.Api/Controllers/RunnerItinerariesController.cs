namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class RunnerItinerariesController : ControllerBase
{
    private readonly IRunnerItineraryService _itineraryService;
    private readonly IMapper _mapper;

    public RunnerItinerariesController(IRunnerItineraryService itineraryService, IMapper mapper)
    {
        _itineraryService = itineraryService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItineraryRequest request, CancellationToken cancellationToken)
    {
        CreateItineraryCommand command = _mapper.Map<CreateItineraryCommand>(request);
        ApiResponse<CreateItineraryResponse> response = await _itineraryService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Itinerary.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetItineraryResponse> response = await _itineraryService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetItinerariesResponse> response = await _itineraryService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItineraryRequest request, CancellationToken cancellationToken)
    {
        UpdateItineraryCommand command = _mapper.Map<UpdateItineraryCommand>(request);
        command.Id = id;
        ApiResponse<UpdateItineraryResponse> response = await _itineraryService.UpdateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteItineraryResponse> response = await _itineraryService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}