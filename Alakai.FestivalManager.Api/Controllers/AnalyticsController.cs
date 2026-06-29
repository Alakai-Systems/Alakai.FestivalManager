namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/dashboard/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, CancellationToken cancellationToken)
    {
        ApiResponse<AnalyticsStatsDto> response = await _analyticsService.GetAnalyticsAsync(startDate, endDate, cancellationToken);

        return Ok(response);
    }
}
