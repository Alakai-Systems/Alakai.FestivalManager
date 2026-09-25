namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
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
            "checkin" => await _reportService.GenerateCheckInReportAsync(editionId, cancellationToken),
            "competitions" => await _reportService.GenerateCompetitionsReportAsync(editionId, cancellationToken),
            "accommodation" => await _reportService.GenerateAccommodationReportAsync(editionId, cancellationToken),
            "accommodation-grid" => await _reportService.GenerateAccommodationGridReportAsync(editionId, cancellationToken),
            "buses" => await _reportService.GenerateBusesReportAsync(editionId, cancellationToken),
            "meals" => await _reportService.GenerateMealsReportAsync(editionId, cancellationToken),
            "production-team" => await _reportService.GenerateProductionTeamReportAsync(editionId, cancellationToken),
            "production-suppliers" => await _reportService.GenerateProductionSuppliersReportAsync(editionId, cancellationToken),
            "production-trips" => await _reportService.GenerateProductionTripsReportAsync(editionId, cancellationToken),
            "production-itineraries" => await _reportService.GenerateProductionItinerariesReportAsync(editionId, cancellationToken),
            "production-accommodation" => await _reportService.GenerateProductionAccommodationReportAsync(editionId, cancellationToken),
            "production-accommodation-grid" => await _reportService.GenerateProductionAccommodationGridReportAsync(editionId, cancellationToken),
            "invoices" => await _reportService.GenerateInvoicesReportAsync(editionId, cancellationToken),
            "financial-summary" => await _reportService.GenerateFinancialSummaryReportAsync(editionId, cancellationToken),
            "early-bird" => await _reportService.GenerateEarlyBirdReportAsync(editionId, cancellationToken),
            "refunds" => await _reportService.GenerateRefundsReportAsync(editionId, cancellationToken),
            "finance-payments" => await _reportService.GenerateFinancePaymentsReportAsync(editionId, cancellationToken),
            "outstanding-balances" => await _reportService.GenerateOutstandingBalancesReportAsync(editionId, cancellationToken),
            _ => []
        };

        if (bytes.Length == 0 && reportType.ToLowerInvariant() is not ("users" or "registrations" or "checkin" or "competitions" or "accommodation" or "accommodation-grid" or "buses" or "meals" or "production-team" or "production-suppliers" or "production-trips" or "production-itineraries" or "production-accommodation" or "production-accommodation-grid" or "invoices" or "financial-summary" or "early-bird" or "refunds" or "finance-payments" or "outstanding-balances"))
        {
            return NotFound($"Unknown report type '{reportType}'.");
        }

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType}.xlsx");
    }
}