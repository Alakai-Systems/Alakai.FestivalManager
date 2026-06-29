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
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLayoutResponse> response = await _emailLayoutService.GetAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateEmailLayoutRequest request, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateEmailLayoutResponse> response = await _emailLayoutService.UpdateAsync(request, cancellationToken);
        return Ok(response);
    }
}
