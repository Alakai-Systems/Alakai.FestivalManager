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
