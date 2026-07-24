namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        if (editionId == Guid.Empty)
        {
            return BadRequest(new { error = "editionId is required." });
        }

        ApiResponse<GetDashboardStatsResponse> response = await _dashboardService.GetStatsAsync(editionId, cancellationToken);

        return Ok(response);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] Guid editionId, [FromQuery] string range, CancellationToken cancellationToken)
    {
        if (editionId == Guid.Empty)
        {
            return BadRequest(new { error = "editionId is required." });
        }

        string normalizedRange = string.IsNullOrWhiteSpace(range) ? "month" : range;

        ApiResponse<List<RevenuePointDto>> response = await _dashboardService.GetRevenueAsync(editionId, normalizedRange, cancellationToken);

        return Ok(response);
    }
}
