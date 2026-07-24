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
    public async Task<IActionResult> Get([FromQuery] Guid festivalId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, CancellationToken cancellationToken)
    {
        if (festivalId == Guid.Empty)
        {
            return BadRequest(new { error = "festivalId is required." });
        }

        ApiResponse<AnalyticsStatsDto> response = await _analyticsService.GetAnalyticsAsync(festivalId, startDate, endDate, cancellationToken);

        return Ok(response);
    }
}
