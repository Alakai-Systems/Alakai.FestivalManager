namespace Alakai.FestivalManager.Admin.Services.Api;

public class DashboardStatsDto
{
    public Guid EditionId { get; set; }
    public List<PassTypeStatDto> PassTypes { get; set; } = [];
    public List<GroupStatDto> Groups { get; set; } = [];
    public List<CompetitionStatDto> Competitions { get; set; } = [];
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

    public DashboardApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        DashboardStatsApiResponse? response = await _httpClient.GetFromJsonAsync<DashboardStatsApiResponse>(
            $"api/dashboard/stats?editionId={editionId}", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load dashboard stats.", response?.Errors);
        }

        return response.Data.Stats;
    }

    public async Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default)
    {
        RevenueApiResponse? response = await _httpClient.GetFromJsonAsync<RevenueApiResponse>(
            $"api/dashboard/revenue?editionId={editionId}&range={range}", cancellationToken);

        return response?.Data ?? [];
    }
}
