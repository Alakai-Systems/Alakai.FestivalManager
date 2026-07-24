namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/email-logs")]
public class EmailLogsController : ControllerBase
{
    private readonly IEmailLogService _emailLogService;
    private readonly IMapper _mapper;

    public EmailLogsController(IEmailLogService emailLogService, IMapper mapper)
    {
        _emailLogService = emailLogService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetEmailLogsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsResponse> response = await _emailLogService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogByIdResponse> response = await _emailLogService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByEditionIdResponse> response = await _emailLogService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByRegistrationIdResponse>>> GetByRegistrationId(Guid registrationId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByRegistrationIdResponse> response = await _emailLogService.GetByRegistrationIdAsync(registrationId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByUserIdResponse>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByUserIdResponse> response = await _emailLogService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateEmailLogResponse>>> Create([FromBody] CreateEmailLogRequest request, CancellationToken cancellationToken)
    {
        CreateEmailLogCommand command = _mapper.Map<CreateEmailLogCommand>(request);
        ApiResponse<CreateEmailLogResponse> response = await _emailLogService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateEmailLogResponse>>> Update(Guid id, [FromBody] UpdateEmailLogRequest request, CancellationToken cancellationToken)
    {
        UpdateEmailLogCommand command = _mapper.Map<UpdateEmailLogCommand>(request);
        ApiResponse<UpdateEmailLogResponse> response = await _emailLogService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteEmailLogResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEmailLogResponse> response = await _emailLogService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
