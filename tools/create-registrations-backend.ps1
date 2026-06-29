# =====================================================================
# Alakai FestivalManager - Dashboard Phase 4a (BACKEND)
# - Competitions: desglose real por nivel (Open/Advanced) y rol, sin
#   columnas de "sin pareja" (eso solo aplica a Registrations).
# - Revenue: endpoint propio con selector week/month/year. Usa UpdatedAt
#   como proxy de fecha de pago real (PaidAt nunca se asigna en el codigo).
# - Google Analytics: GetStatsAsync ahora acepta startDate/endDate reales
#   en vez de "firstDayOfMonth" fijo, para poder variar por edicion.
#
# USO: desde la raiz del repo -> .\dashboard-phase4a-backend.ps1
# =====================================================================

$ErrorActionPreference = "Stop"
Write-Host "Trabajando en: $(Get-Location)" -ForegroundColor Cyan

function Write-FileContent {
    param([string]$Path, [string]$Content)
    $dir = Split-Path -Path $Path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Set-Content -Path $Path -Value $Content -Encoding UTF8
    Write-Host "  Escrito: $Path" -ForegroundColor Green
}

# ---------------------------------------------------------------------
# 1. Application - DTOs
# ---------------------------------------------------------------------

$dashboardStatsDto = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Contracts.DTOs;

public class LevelStatDto
{
    public Guid? LevelId { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int Purchased { get; set; }
    public int Individual { get; set; }
    public int Follower { get; set; }
    public int Leader { get; set; }
    public int FollowerWithoutPartner { get; set; }
    public int LeaderWithoutPartner { get; set; }
}

public class PassTypeStatDto
{
    public Guid PassTypeId { get; set; }
    public string PassTypeName { get; set; } = string.Empty;
    public int Purchased { get; set; }
    public int FullyPaid { get; set; }
    public int PendingPayment { get; set; }
    public int Unpaid { get; set; }
    public bool HasRoleBreakdown { get; set; }
    public List<LevelStatDto> Levels { get; set; } = [];
}

public class GroupStatDto
{
    public string GroupName { get; set; } = string.Empty;
    public int Purchased { get; set; }
}

/// <summary>
/// Breakdown of a competition's entries by capacity level (e.g. Open / Advanced
/// for Mix &amp; Match). For competitions without levels (e.g. Solo Jazz), a single
/// entry with LevelLabel = "All" is returned.
/// </summary>
public class CompetitionLevelStatDto
{
    public string LevelLabel { get; set; } = string.Empty;
    public int Individual { get; set; }
    public int Follower { get; set; }
    public int Leader { get; set; }
}

public class CompetitionStatDto
{
    public Guid CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public CompetitionFormat Format { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public int Total { get; set; }
    public List<CompetitionLevelStatDto> Levels { get; set; } = [];
}

public class DashboardStatsDto
{
    public Guid EditionId { get; set; }
    public List<PassTypeStatDto> PassTypes { get; set; } = [];
    public List<GroupStatDto> Groups { get; set; } = [];
    public List<CompetitionStatDto> Competitions { get; set; } = [];
}

public class RevenuePointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Contracts/DTOs/DashboardStatsDto.cs" -Content $dashboardStatsDto

$analyticsDtos = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Contracts.DTOs;

public class AnalyticsOverviewDto
{
    public long TotalViews { get; set; }
    public long ActiveUsers { get; set; }
    public long EventCount { get; set; }
    public long NewUsers { get; set; }
}

public class AnalyticsCountryStatDto
{
    public string Country { get; set; } = string.Empty;
    public long ActiveUsers { get; set; }
}

public class AnalyticsPageStatDto
{
    public string PagePath { get; set; } = string.Empty;
    public long Views { get; set; }
}

public class AnalyticsStatsDto
{
    public bool IsAvailable { get; set; }
    public string? ErrorMessage { get; set; }
    public AnalyticsOverviewDto Overview { get; set; } = new();
    public List<AnalyticsCountryStatDto> TopCountries { get; set; } = [];
    public List<AnalyticsPageStatDto> TopPages { get; set; } = [];
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Contracts/DTOs/AnalyticsStatsDto.cs" -Content $analyticsDtos

# ---------------------------------------------------------------------
# 2. Application - interfaces (repository + analytics client + services)
# ---------------------------------------------------------------------

$idashboardRepo = @'
namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);

    /// <param name="range">"week" (last 7 days, daily), "month" (last 30 days, daily) or "year" (last 12 months, monthly).</param>
    Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default);
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Interfaces/Repositories/IDashboardRepository.cs" -Content $idashboardRepo

$ianalyticsClient = @'
namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IAnalyticsClient
{
    Task<AnalyticsStatsDto> GetStatsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Interfaces/Services/IAnalyticsClient.cs" -Content $ianalyticsClient

$idashboardService = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IDashboardService
{
    Task<ApiResponse<GetDashboardStatsResponse>> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RevenuePointDto>>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default);
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Services/IDashboardService.cs" -Content $idashboardService

$dashboardService = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<ApiResponse<GetDashboardStatsResponse>> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        DashboardStatsDto stats = await _dashboardRepository.GetStatsAsync(editionId, cancellationToken);

        return new ApiResponse<GetDashboardStatsResponse>
        {
            Success = true,
            Message = "Dashboard stats retrieved successfully",
            Data = new GetDashboardStatsResponse { Stats = stats },
            Errors = []
        };
    }

    public async Task<ApiResponse<List<RevenuePointDto>>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default)
    {
        List<RevenuePointDto> points = await _dashboardRepository.GetRevenueAsync(editionId, range, cancellationToken);

        return new ApiResponse<List<RevenuePointDto>>
        {
            Success = true,
            Message = "Revenue retrieved successfully",
            Data = points,
            Errors = []
        };
    }
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Services/DashboardService.cs" -Content $dashboardService

$ianalyticsService = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public interface IAnalyticsService
{
    Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Services/IAnalyticsService.cs" -Content $ianalyticsService

$analyticsService = @'
namespace Alakai.FestivalManager.Application.Features.Dashboard.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsClient _analyticsClient;

    public AnalyticsService(IAnalyticsClient analyticsClient)
    {
        _analyticsClient = analyticsClient;
    }

    public async Task<ApiResponse<AnalyticsStatsDto>> GetAnalyticsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        AnalyticsStatsDto stats = await _analyticsClient.GetStatsAsync(startDate, endDate, cancellationToken);

        return new ApiResponse<AnalyticsStatsDto>
        {
            Success = true,
            Message = stats.IsAvailable ? "Analytics retrieved successfully" : "Analytics unavailable",
            Data = stats,
            Errors = []
        };
    }
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Application/Features/Dashboard/Services/AnalyticsService.cs" -Content $analyticsService

# ---------------------------------------------------------------------
# 3. Infrastructure - Repository
# ---------------------------------------------------------------------

$dashboardRepoImpl = @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly FestivalManagerDbContext _context;

    public DashboardRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        List<Registration> registrations = await _context.Registrations
            .Where(r => r.EditionId == editionId && r.IsActive && r.Status != RegistrationStatus.Cancelled)
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.DiscountCode)
            .ToListAsync(cancellationToken);

        List<PassType> passTypes = await _context.PassTypes
            .Where(p => p.EditionId == editionId && p.IsActive)
            .Include(p => p.Levels)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        List<PassTypeStatDto> passTypeStats = [];

        foreach (PassType passType in passTypes)
        {
            List<Registration> passTypeRegistrations = registrations
                .Where(r => r.PassTypeId == passType.Id)
                .ToList();

            bool hasRoleBreakdown = passTypeRegistrations.Any(r => r.DanceRole == DanceRole.Follower || r.DanceRole == DanceRole.Leader);

            List<LevelStatDto> levelStats = [];

            IEnumerable<Level> levelsOrdered = passType.Levels
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder);

            foreach (Level level in levelsOrdered)
            {
                List<Registration> levelRegistrations = passTypeRegistrations
                    .Where(r => r.LevelId == level.Id)
                    .ToList();

                levelStats.Add(BuildLevelStat(level.Id, level.Name, levelRegistrations));
            }

            List<Registration> registrationsWithoutLevel = passTypeRegistrations
                .Where(r => r.LevelId is null)
                .ToList();

            if (registrationsWithoutLevel.Count > 0)
            {
                levelStats.Add(BuildLevelStat(null, passType.Name, registrationsWithoutLevel));
            }

            passTypeStats.Add(new PassTypeStatDto
            {
                PassTypeId = passType.Id,
                PassTypeName = passType.Name,
                Purchased = passTypeRegistrations.Count,
                FullyPaid = passTypeRegistrations.Count(r => r.PaymentStatus == PaymentStatus.Paid),
                PendingPayment = passTypeRegistrations.Count(r => r.PaymentStatus == PaymentStatus.Pending),
                Unpaid = passTypeRegistrations.Count(r => r.PaymentStatus == PaymentStatus.Unpaid || r.PaymentStatus == PaymentStatus.Failed),
                HasRoleBreakdown = hasRoleBreakdown,
                Levels = levelStats
            });
        }

        List<GroupStatDto> groupStats = registrations
            .Where(r => r.DiscountCode is not null)
            .GroupBy(r => r.DiscountCode!.Name)
            .Select(g => new GroupStatDto { GroupName = g.Key, Purchased = g.Count() })
            .OrderByDescending(g => g.Purchased)
            .ToList();

        // --- Competitions: real breakdown by capacity level (Open/Advanced) + role ---
        List<Competition> competitions = await _context.Competitions
            .Where(c => c.EditionId == editionId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        List<Guid> competitionIds = competitions.Select(c => c.Id).ToList();

        List<CompetitionEntry> competitionEntries = competitionIds.Count == 0
            ? []
            : await _context.CompetitionEntries
                .Where(e => competitionIds.Contains(e.CompetitionId) && e.IsActive && e.Status != CompetitionEntryStatus.Cancelled)
                .ToListAsync(cancellationToken);

        List<CompetitionStatDto> competitionStats = [];

        foreach (Competition competition in competitions)
        {
            List<CompetitionEntry> entries = competitionEntries
                .Where(e => e.CompetitionId == competition.Id)
                .ToList();

            List<CompetitionLevelStatDto> levelStats = entries
                .GroupBy(e => e.MixAndMatchLevel)
                .Select(g => new CompetitionLevelStatDto
                {
                    LevelLabel = g.Key.HasValue ? g.Key.Value.ToString() : "All",
                    Individual = g.Count(e => e.DanceRole == DanceRole.Individual),
                    Follower = g.Count(e => e.DanceRole == DanceRole.Follower),
                    Leader = g.Count(e => e.DanceRole == DanceRole.Leader)
                })
                .OrderBy(l => l.LevelLabel == "All" ? 0 : 1)
                .ThenBy(l => l.LevelLabel)
                .ToList();

            competitionStats.Add(new CompetitionStatDto
            {
                CompetitionId = competition.Id,
                CompetitionName = competition.Name,
                Format = competition.Format,
                RequiresPartner = competition.RequiresPartner,
                RequiresRole = competition.RequiresRole,
                Total = entries.Count,
                Levels = levelStats
            });
        }

        return new DashboardStatsDto
        {
            EditionId = editionId,
            PassTypes = passTypeStats,
            Groups = groupStats,
            Competitions = competitionStats
        };
    }

    /// <summary>
    /// "Sin pareja" matches RegistrationPartnerService.LinkPartnerAsync's definition of a
    /// failed match: PartnerEmail was specified but PartnerRegistrationId ended up null.
    /// RegistrationStatus.WaitingPartner is never assigned anywhere, so it is not used here.
    /// </summary>
    private static LevelStatDto BuildLevelStat(Guid? levelId, string levelName, List<Registration> levelRegistrations)
    {
        bool IsWithoutPartner(Registration r) => !string.IsNullOrWhiteSpace(r.PartnerEmail) && r.PartnerRegistrationId is null;

        return new LevelStatDto
        {
            LevelId = levelId,
            LevelName = levelName,
            Purchased = levelRegistrations.Count,
            Individual = levelRegistrations.Count(r => r.DanceRole == DanceRole.Individual),
            Follower = levelRegistrations.Count(r => r.DanceRole == DanceRole.Follower),
            Leader = levelRegistrations.Count(r => r.DanceRole == DanceRole.Leader),
            FollowerWithoutPartner = levelRegistrations.Count(r => r.DanceRole == DanceRole.Follower && IsWithoutPartner(r)),
            LeaderWithoutPartner = levelRegistrations.Count(r => r.DanceRole == DanceRole.Leader && IsWithoutPartner(r))
        };
    }

    /// <summary>
    /// Revenue is grouped by UpdatedAt as a proxy for "date paid", because the
    /// Registration.PaidAt field exists in the schema but is never assigned by any
    /// handler in the codebase. UpdatedAt is the closest reliable signal of when the
    /// PaymentStatus last changed. This is a known limitation, not a perfect audit trail.
    /// </summary>
    public async Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default)
    {
        List<Registration> paidRegistrations = await _context.Registrations
            .Where(r => r.EditionId == editionId && r.PaymentStatus == PaymentStatus.Paid)
            .ToListAsync(cancellationToken);

        DateTime Effective(Registration r) => r.UpdatedAt ?? r.CreatedAt;

        if (string.Equals(range, "year", StringComparison.OrdinalIgnoreCase))
        {
            List<RevenuePointDto> points = [];

            for (int i = 11; i >= 0; i--)
            {
                DateTime monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
                DateTime monthEnd = monthStart.AddMonths(1);

                decimal amount = paidRegistrations
                    .Where(r => Effective(r) >= monthStart && Effective(r) < monthEnd)
                    .Sum(r => r.FinalPrice);

                points.Add(new RevenuePointDto { Label = monthStart.ToString("MMM yyyy"), Amount = amount });
            }

            return points;
        }

        int days = string.Equals(range, "week", StringComparison.OrdinalIgnoreCase) ? 6 : 29;
        DateOnly startDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        List<RevenuePointDto> dailyPoints = [];

        for (DateOnly day = startDay; day <= today; day = day.AddDays(1))
        {
            decimal amount = paidRegistrations
                .Where(r => DateOnly.FromDateTime(Effective(r)) == day)
                .Sum(r => r.FinalPrice);

            string label = string.Equals(range, "week", StringComparison.OrdinalIgnoreCase)
                ? day.ToString("ddd")
                : day.ToString("dd/MM");

            dailyPoints.Add(new RevenuePointDto { Label = label, Amount = amount });
        }

        return dailyPoints;
    }
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Infrastructure/Repositories/DashboardRepository.cs" -Content $dashboardRepoImpl

# ---------------------------------------------------------------------
# 4. Infrastructure - GoogleAnalyticsClient (acepta rango de fechas real)
# ---------------------------------------------------------------------

$gaClient = @'
using Google.Analytics.Data.V1Beta;
using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Infrastructure.Analytics;

public class GoogleAnalyticsClient : IAnalyticsClient
{
    private readonly GoogleAnalyticsOptions _options;
    private readonly ILogger<GoogleAnalyticsClient> _logger;

    public GoogleAnalyticsClient(IOptions<GoogleAnalyticsOptions> options, ILogger<GoogleAnalyticsClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AnalyticsStatsDto> GetStatsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PropertyId) || string.IsNullOrWhiteSpace(_options.CredentialsPath))
        {
            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = "Google Analytics is not configured (missing PropertyId or CredentialsPath)."
            };
        }

        try
        {
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = _options.CredentialsPath
            }.Build();

            string property = $"properties/{_options.PropertyId}";
            DateRange range = new()
            {
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };

            RunReportRequest overviewRequest = new()
            {
                Property = property,
                DateRanges = { range },
                Metrics =
                {
                    new Metric { Name = "screenPageViews" },
                    new Metric { Name = "activeUsers" },
                    new Metric { Name = "eventCount" },
                    new Metric { Name = "newUsers" }
                }
            };

            RunReportResponse overviewResponse = await client.RunReportAsync(overviewRequest, cancellationToken);

            AnalyticsOverviewDto overview = new();

            if (overviewResponse.Rows.Count > 0)
            {
                Row row = overviewResponse.Rows[0];
                overview.TotalViews = ParseLong(row.MetricValues[0].Value);
                overview.ActiveUsers = ParseLong(row.MetricValues[1].Value);
                overview.EventCount = ParseLong(row.MetricValues[2].Value);
                overview.NewUsers = ParseLong(row.MetricValues[3].Value);
            }

            RunReportRequest countriesRequest = new()
            {
                Property = property,
                DateRanges = { range },
                Dimensions = { new Dimension { Name = "country" } },
                Metrics = { new Metric { Name = "activeUsers" } },
                OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "activeUsers" }, Desc = true } },
                Limit = 5
            };

            RunReportResponse countriesResponse = await client.RunReportAsync(countriesRequest, cancellationToken);

            List<AnalyticsCountryStatDto> topCountries = countriesResponse.Rows
                .Select(r => new AnalyticsCountryStatDto
                {
                    Country = r.DimensionValues[0].Value,
                    ActiveUsers = ParseLong(r.MetricValues[0].Value)
                })
                .ToList();

            RunReportRequest pagesRequest = new()
            {
                Property = property,
                DateRanges = { range },
                Dimensions = { new Dimension { Name = "pagePath" } },
                Metrics = { new Metric { Name = "screenPageViews" } },
                OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "screenPageViews" }, Desc = true } },
                Limit = 5
            };

            RunReportResponse pagesResponse = await client.RunReportAsync(pagesRequest, cancellationToken);

            List<AnalyticsPageStatDto> topPages = pagesResponse.Rows
                .Select(r => new AnalyticsPageStatDto
                {
                    PagePath = r.DimensionValues[0].Value,
                    Views = ParseLong(r.MetricValues[0].Value)
                })
                .ToList();

            return new AnalyticsStatsDto
            {
                IsAvailable = true,
                Overview = overview,
                TopCountries = topCountries,
                TopPages = topPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Google Analytics data.");

            return new AnalyticsStatsDto
            {
                IsAvailable = false,
                ErrorMessage = "Could not retrieve Google Analytics data. Check server logs for details."
            };
        }
    }

    private static long ParseLong(string value) => long.TryParse(value, out long result) ? result : 0;
}
'@
Write-FileContent -Path "Alakai.FestivalManager.Infrastructure/Analytics/GoogleAnalyticsClient.cs" -Content $gaClient

# ---------------------------------------------------------------------
# 5. Api - Controllers
# ---------------------------------------------------------------------

$dashboardController = @'
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
'@
Write-FileContent -Path "Alakai.FestivalManager.Api/Controllers/DashboardController.cs" -Content $dashboardController

$analyticsController = @'
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
'@
Write-FileContent -Path "Alakai.FestivalManager.Api/Controllers/AnalyticsController.cs" -Content $analyticsController

Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "Backend listo. Sigue con dashboard-phase4b-frontend.ps1" -ForegroundColor Cyan
Write-Host "No hay cambios de DI nuevos respecto a la fase anterior." -ForegroundColor Yellow
Write-Host "=====================================================================" -ForegroundColor Cyan