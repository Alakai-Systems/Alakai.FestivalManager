namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/competitions")]
public class CompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;

    public CompetitionsController(ICompetitionService competitionService)
    {
        _competitionService = competitionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetCompetitionsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionsResponse> response = await _competitionService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionByIdResponse> response = await _competitionService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionsByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionsByEditionIdResponse> response = await _competitionService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateCompetitionResponse>>> Create([FromBody] CreateCompetitionCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<CreateCompetitionResponse> response = await _competitionService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateCompetitionResponse>>> Update(Guid id, [FromBody] UpdateCompetitionCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateCompetitionResponse> response = await _competitionService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteCompetitionResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteCompetitionResponse> response = await _competitionService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
