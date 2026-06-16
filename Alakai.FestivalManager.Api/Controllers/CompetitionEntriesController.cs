namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/competition-entries")]
public class CompetitionEntriesController : ControllerBase
{
    private readonly ICompetitionEntryService _competitionEntryService;

    public CompetitionEntriesController(ICompetitionEntryService competitionEntryService)
    {
        _competitionEntryService = competitionEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesResponse> response = await _competitionEntryService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntryByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntryByIdResponse> response = await _competitionEntryService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-competition/{competitionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>>> GetByCompetitionId(Guid competitionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesByCompetitionIdResponse> response = await _competitionEntryService.GetByCompetitionIdAsync(competitionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>>> GetByRegistrationId(Guid registrationId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesByRegistrationIdResponse> response = await _competitionEntryService.GetByRegistrationIdAsync(registrationId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateCompetitionEntryResponse>>> Create([FromBody] CreateCompetitionEntryCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<CreateCompetitionEntryResponse> response = await _competitionEntryService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateCompetitionEntryResponse>>> Update(Guid id, [FromBody] UpdateCompetitionEntryCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateCompetitionEntryResponse> response = await _competitionEntryService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteCompetitionEntryResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteCompetitionEntryResponse> response = await _competitionEntryService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
