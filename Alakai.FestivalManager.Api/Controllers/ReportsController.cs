namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("{reportType}")]
    public async Task<IActionResult> GetReport(string reportType, [FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        if (editionId == Guid.Empty)
        {
            return BadRequest("editionId is required.");
        }

        byte[] bytes = reportType.ToLowerInvariant() switch
        {
            "users" => await _reportService.GenerateUsersReportAsync(editionId, cancellationToken),
            "registrations" => await _reportService.GenerateRegistrationsReportAsync(editionId, cancellationToken),
            "competitions" => await _reportService.GenerateCompetitionsReportAsync(editionId, cancellationToken),
            "accommodation" => await _reportService.GenerateAccommodationReportAsync(editionId, cancellationToken),
            "accommodation-grid" => await _reportService.GenerateAccommodationGridReportAsync(editionId, cancellationToken),
            "buses" => await _reportService.GenerateBusesReportAsync(editionId, cancellationToken),
            "meals" => await _reportService.GenerateMealsReportAsync(editionId, cancellationToken),
            _ => []
        };

        if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals"))
        {
            return NotFound($"Unknown report type '{reportType}'.");
        }

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType}.xlsx");
    }
}