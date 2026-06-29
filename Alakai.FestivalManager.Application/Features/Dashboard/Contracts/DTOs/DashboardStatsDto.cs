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
