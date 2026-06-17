namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/email-templates")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IMapper _mapper;

    public EmailTemplatesController(IEmailTemplateService emailTemplateService, IMapper mapper)
    {
        _emailTemplateService = emailTemplateService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        CreateEmailTemplateCommand command = _mapper.Map<CreateEmailTemplateCommand>(request);
        ApiResponse<CreateEmailTemplateResponse> response = await _emailTemplateService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(Create), response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailTemplateByIdResponse> response = await _emailTemplateService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailTemplatesResponse> response = await _emailTemplateService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailTemplatesByEditionIdResponse> response = await _emailTemplateService.GetByEditionIdAsync(editionId, cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        UpdateEmailTemplateCommand command = _mapper.Map<UpdateEmailTemplateCommand>(request);
        command.Id = id;

        ApiResponse<UpdateEmailTemplateResponse> response = await _emailTemplateService.UpdateAsync(command, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEmailTemplateResponse> response = await _emailTemplateService.DeleteAsync(id, cancellationToken);

        return Ok(response);
    }
}
