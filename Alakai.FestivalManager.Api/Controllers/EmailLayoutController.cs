namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/email-layout")]
public class EmailLayoutController : ControllerBase
{
    private readonly IEmailLayoutService _emailLayoutService;

    public EmailLayoutController(IEmailLayoutService emailLayoutService)
    {
        _emailLayoutService = emailLayoutService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLayoutsResponse> response = await _emailLayoutService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailLayoutRequest request, CancellationToken cancellationToken)
    {
        ApiResponse<CreateEmailLayoutResponse> response = await _emailLayoutService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmailLayoutRequest request, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateEmailLayoutResponse> response = await _emailLayoutService.UpdateAsync(id, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEmailLayoutResponse> response = await _emailLayoutService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}