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
                PartiallyPaid = passTypeRegistrations.Count(r => r.PaymentStatus == PaymentStatus.PartiallyPaid),
                Unpaid = passTypeRegistrations.Count(r => r.PaymentStatus == PaymentStatus.Unpaid || r.PaymentStatus == PaymentStatus.Failed || r.PaymentStatus == PaymentStatus.Pending),
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

        // --- Competitions: level list from configured CompetitionCapacity / CompetitionLevel,
        // so every configured level (e.g. Open AND Advanced) always shows, even with 0 entries.
        // Each entry's level is derived via its CompetitionCapacityId (entries no longer
        // denormalize a level field of their own). ---
        List<Competition> competitions = await _context.Competitions
            .Where(c => c.EditionId == editionId && c.IsActive)
            .Include(c => c.Capacities)
            .Include(c => c.Levels)
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

            Dictionary<Guid, Guid?> capacityIdToLevelId = competition.Capacities
                .ToDictionary(c => c.Id, c => c.CompetitionLevelId);

            List<CompetitionLevel> orderedLevels = competition.Levels
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ToList();

            List<Guid?> configuredLevelIds = competition.Capacities
                .Where(c => c.IsActive)
                .Select(c => c.CompetitionLevelId)
                .Distinct()
                .OrderBy(id => id.HasValue ? orderedLevels.FindIndex(l => l.Id == id.Value) : -1)
                .ToList();

            if (configuredLevelIds.Count == 0)
            {
                configuredLevelIds = [null];
            }

            List<CompetitionLevelStatDto> levelStats = configuredLevelIds
                .Select(levelKey =>
                {
                    List<CompetitionEntry> levelEntries = entries
                        .Where(e => capacityIdToLevelId.TryGetValue(e.CompetitionCapacityId, out Guid? lvl) && lvl == levelKey)
                        .ToList();

                    int individual = levelEntries.Count(e => e.DanceRole == DanceRole.Individual);
                    int follower = levelEntries.Count(e => e.DanceRole == DanceRole.Follower);
                    int leader = levelEntries.Count(e => e.DanceRole == DanceRole.Leader);

                    string label = levelKey.HasValue
                        ? orderedLevels.FirstOrDefault(l => l.Id == levelKey.Value)?.Name ?? "Unknown"
                        : "All";

                    return new CompetitionLevelStatDto
                    {
                        LevelLabel = label,
                        Individual = individual,
                        Follower = follower,
                        Leader = leader,
                        Total = competition.RequiresRole ? follower + leader : individual
                    };
                })
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
    /// Returns a CUMULATIVE running total within the selected period (resets to 0 at
    /// the start of the window), since revenue only ever adds and the chart is meant to
    /// be ascending. Grouped by UpdatedAt as a proxy for "date paid" (Registration.PaidAt
    /// exists in the schema but is never assigned anywhere in the codebase -- a known
    /// limitation, not a perfect audit trail).
    /// TODO: once the Redsys payment integration exists, this should be revisited to use
    /// the real payment confirmation timestamp and to support true partial payments,
    /// instead of the current Pending/Paid/Unpaid/Failed binary PaymentStatus model.
    /// </summary>
    public async Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, CancellationToken cancellationToken = default)
    {
        List<Registration> paidRegistrations = await _context.Registrations
            .Where(r => r.EditionId == editionId && (r.PaymentStatus == PaymentStatus.Paid || r.PaymentStatus == PaymentStatus.PartiallyPaid))
            .ToListAsync(cancellationToken);

        DateTime Effective(Registration r) => r.UpdatedAt ?? r.CreatedAt;

        List<RevenuePointDto> dailyOrMonthlyPoints;

        if (string.Equals(range, "year", StringComparison.OrdinalIgnoreCase))
        {
            List<RevenuePointDto> points = [];

            for (int i = 11; i >= 0; i--)
            {
                DateTime monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
                DateTime monthEnd = monthStart.AddMonths(1);

                decimal amount = paidRegistrations
                    .Where(r => Effective(r) >= monthStart && Effective(r) < monthEnd)
                    .Sum(r => r.AmountPaid);

                points.Add(new RevenuePointDto
                {
                    Label = monthStart.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    Amount = amount
                });
            }

            dailyOrMonthlyPoints = points;
        }
        else
        {
            int days = string.Equals(range, "week", StringComparison.OrdinalIgnoreCase) ? 6 : 29;
            DateOnly startDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            List<RevenuePointDto> points = [];

            for (DateOnly day = startDay; day <= today; day = day.AddDays(1))
            {
                decimal amount = paidRegistrations
                    .Where(r => DateOnly.FromDateTime(Effective(r)) == day)
                    .Sum(r => r.AmountPaid);

                string label = string.Equals(range, "week", StringComparison.OrdinalIgnoreCase)
                    ? day.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)
                    : day.ToString("dd/MM");

                points.Add(new RevenuePointDto { Label = label, Amount = amount });
            }

            dailyOrMonthlyPoints = points;
        }

        decimal running = 0;
        List<RevenuePointDto> cumulative = [];

        foreach (RevenuePointDto point in dailyOrMonthlyPoints)
        {
            running += point.Amount;
            cumulative.Add(new RevenuePointDto { Label = point.Label, Amount = running });
        }

        return cumulative;
    }
}
