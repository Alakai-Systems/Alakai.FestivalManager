using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class PaymentStatusBreakdownDto
{
    public decimal PaidAmount { get; set; }
    public decimal PartiallyPaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}

public class AgedPendingPaymentDto
{
    public string Bucket { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class DiscountCodeCostDto
{
    public string CodeName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
}

public class PaymentPlanMixDto
{
    public string PlanName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardStatsDto
{
    public Guid EditionId { get; set; }
    public List<PassTypeStatDto> PassTypes { get; set; } = [];
    public List<GroupStatDto> Groups { get; set; } = [];
    public List<CompetitionStatDto> Competitions { get; set; } = [];
    public PaymentStatusBreakdownDto PaymentStatusBreakdown { get; set; } = new();
    public List<AgedPendingPaymentDto> AgedPendingPayments { get; set; } = [];
    public List<DiscountCodeCostDto> DiscountCodeCosts { get; set; } = [];
    public decimal TotalManagementFees { get; set; }
    public int ManagementFeeRegistrationCount { get; set; }
    public List<PaymentPlanMixDto> PaymentPlanMix { get; set; } = [];
    public decimal TotalRevenue { get; set; }
    public List<RegistrationTrendPointDto> RegistrationsOverTime { get; set; } = [];
}

public class RegistrationTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PassTypeStatDto
{
    public Guid PassTypeId { get; set; }
    public string PassTypeName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Purchased { get; set; }
    public int PartiallyPaid { get; set; }
    public int FullyPaid { get; set; }
    public int PendingPayment { get; set; }
    public int Unpaid { get; set; }
    public bool HasRoleBreakdown { get; set; }
    public List<LevelStatDto> Levels { get; set; } = [];
}

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

public class GroupStatDto
{
    public string GroupName { get; set; } = string.Empty;
    public int Purchased { get; set; }
}

public class CompetitionLevelStatDto
{
    public string LevelLabel { get; set; } = string.Empty;
    public int Individual { get; set; }
    public int Follower { get; set; }
    public int Leader { get; set; }
    public int Total { get; set; }
}

public class CompetitionStatDto
{
    public Guid CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public int Format { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public int Total { get; set; }
    public List<CompetitionLevelStatDto> Levels { get; set; } = [];
}

public class RevenuePointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

internal class DashboardStatsApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DashboardStatsResponseData? Data { get; set; }
    public List<string>? Errors { get; set; }
}

internal class DashboardStatsResponseData
{
    public DashboardStatsDto Stats { get; set; } = new();
}

internal class RevenueApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<RevenuePointDto>? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class DashboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public DashboardApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
    {
        _httpClient = httpClient;
        _adminTokenProvider = adminTokenProvider;
    }

    private async Task AttachAuthHeaderAsync()
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        DashboardStatsApiResponse? response = await _httpClient.GetFromJsonAsync<DashboardStatsApiResponse>(
            $"api/dashboard/stats?editionId={editionId}", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load dashboard stats.", response?.Errors);
        }

        return response.Data.Stats;
    }

    public async Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, int offset = 0, DateOnly? customStart = null, DateOnly? customEnd = null, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        string query = $"api/dashboard/revenue?editionId={editionId}&range={range}&offset={offset}";

        if (customStart.HasValue && customEnd.HasValue)
        {
            query += $"&customStart={customStart.Value:yyyy-MM-dd}&customEnd={customEnd.Value:yyyy-MM-dd}";
        }

        RevenueApiResponse? response = await _httpClient.GetFromJsonAsync<RevenueApiResponse>(query, cancellationToken);

        return response?.Data ?? [];
    }
}
