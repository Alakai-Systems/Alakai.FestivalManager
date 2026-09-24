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
                Revenue = passTypeRegistrations.Sum(r => r.AmountPaid),
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

        List<DiscountCodeCostDto> discountCodeCosts = registrations
            .Where(r => r.DiscountCode is not null && r.DiscountAmount > 0)
            .GroupBy(r => r.DiscountCode!.Name)
            .Select(g => new DiscountCodeCostDto
            {
                CodeName = g.Key,
                UsageCount = g.Count(),
                TotalDiscountAmount = g.Sum(r => r.DiscountAmount)
            })
            .OrderByDescending(d => d.TotalDiscountAmount)
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

        PaymentStatusBreakdownDto paymentStatusBreakdown = new()
        {
            PaidAmount = registrations.Where(r => r.PaymentStatus == PaymentStatus.Paid).Sum(r => r.FinalPrice),
            PartiallyPaidAmount = registrations.Where(r => r.PaymentStatus == PaymentStatus.PartiallyPaid).Sum(r => r.FinalPrice),
            PendingAmount = registrations.Where(r => r.PaymentStatus == PaymentStatus.Unpaid || r.PaymentStatus == PaymentStatus.Failed || r.PaymentStatus == PaymentStatus.Pending).Sum(r => r.FinalPrice)
        };

        List<AgedPendingPaymentDto> agedPendingPayments = BuildAgedPendingPayments(registrations, DateTime.UtcNow);

        decimal totalManagementFees = registrations.Sum(r => r.ManagementFee);
        int managementFeeRegistrationCount = registrations.Count(r => r.ManagementFee > 0);
        decimal totalRevenue = registrations.Sum(r => r.AmountPaid);
        List<RegistrationTrendPointDto> registrationsOverTime = BuildRegistrationTrendPoints(registrations, 8);

        List<PaymentPlanMixDto> paymentPlanMix = registrations
            .GroupBy(r => r.PaymentPlan)
            .Select(g => new PaymentPlanMixDto
            {
                PlanName = g.Key switch
                {
                    PaymentPlan.FullOnline => "Full payment",
                    PaymentPlan.SplitFiftyFifty => "50/50 split",
                    PaymentPlan.DeferredTenDays => "Deferred (10 days)",
                    _ => g.Key.ToString()
                },
                Count = g.Count()
            })
            .OrderByDescending(p => p.Count)
            .ToList();

        return new DashboardStatsDto
        {
            EditionId = editionId,
            PassTypes = passTypeStats,
            Groups = groupStats,
            Competitions = competitionStats,
            PaymentStatusBreakdown = paymentStatusBreakdown,
            AgedPendingPayments = agedPendingPayments,
            DiscountCodeCosts = discountCodeCosts,
            TotalManagementFees = totalManagementFees,
            ManagementFeeRegistrationCount = managementFeeRegistrationCount,
            PaymentPlanMix = paymentPlanMix,
            TotalRevenue = totalRevenue,
            RegistrationsOverTime = registrationsOverTime
        };
    }

    /// <summary>
    /// Un punto por semana natural con el numero de inscripciones nuevas
    /// (por CreatedAt) en esa semana, de mas antigua a mas reciente. A
    /// diferencia de BuildWeeklyPoints (que suma dinero cobrado por PaidAt),
    /// esto cuenta gente que se apunta, independientemente de si ya ha
    /// pagado o no.
    /// </summary>
    private static List<RegistrationTrendPointDto> BuildRegistrationTrendPoints(List<Registration> registrations, int weekCount)
    {
        DateTime today = DateTime.UtcNow.Date;
        int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        DateTime thisWeekStart = today.AddDays(-daysSinceMonday);

        List<RegistrationTrendPointDto> points = [];

        for (int i = weekCount - 1; i >= 0; i--)
        {
            DateTime weekStart = thisWeekStart.AddDays(-7 * i);
            DateTime weekEnd = weekStart.AddDays(7);

            int count = registrations.Count(r => r.CreatedAt >= weekStart && r.CreatedAt < weekEnd);

            points.Add(new RegistrationTrendPointDto
            {
                Label = weekStart.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture),
                Count = count
            });
        }

        return points;
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
    /// Cobros pendientes con antiguedad: registrations con PaymentDueAt ya vencido
    /// (en el pasado) y que todavia deben dinero (no Paid, no Refunded), agrupadas
    /// en tramos de dias de retraso. Amount es el saldo pendiente real (FinalPrice
    /// menos lo ya cobrado en AmountPaid), no el precio total.
    /// </summary>
    private static List<AgedPendingPaymentDto> BuildAgedPendingPayments(List<Registration> registrations, DateTime now)
    {
        List<Registration> overdue = registrations
            .Where(r => r.PaymentDueAt.HasValue && r.PaymentDueAt.Value < now
                && r.PaymentStatus != PaymentStatus.Paid && r.PaymentStatus != PaymentStatus.Refunded)
            .ToList();

        (string Label, int MinDays, int? MaxDays)[] buckets =
        [
            ("0-7 days", 0, 7),
            ("8-30 days", 8, 30),
            ("31-60 days", 31, 60),
            ("60+ days", 61, null)
        ];

        List<AgedPendingPaymentDto> result = [];

        foreach ((string Label, int MinDays, int? MaxDays) bucket in buckets)
        {
            List<Registration> inBucket = overdue
                .Where(r =>
                {
                    int daysOverdue = (int)(now - r.PaymentDueAt!.Value).TotalDays;
                    return daysOverdue >= bucket.MinDays && (bucket.MaxDays is null || daysOverdue <= bucket.MaxDays);
                })
                .ToList();

            result.Add(new AgedPendingPaymentDto
            {
                Bucket = bucket.Label,
                Count = inBucket.Count,
                Amount = inBucket.Sum(r => r.FinalPrice - r.AmountPaid)
            });
        }

        return result;
    }

    /// <summary>
    /// Returns a CUMULATIVE running total within the selected period (resets to 0 at
    /// the start of the window), since revenue only ever adds and the chart is meant to
    /// be ascending. Grouped by Registration.PaidAt, the real payment confirmation
    /// timestamp set by PaymentService when Redsys confirms the charge. Falls back to
    /// UpdatedAt/CreatedAt only for historical rows where PaidAt was never set.
    /// </summary>
    public async Task<List<RevenuePointDto>> GetRevenueAsync(Guid editionId, string range, int offset = 0, DateOnly? customStart = null, DateOnly? customEnd = null, CancellationToken cancellationToken = default)
    {
        List<Registration> paidRegistrations = await _context.Registrations
            .Where(r => r.EditionId == editionId && (r.PaymentStatus == PaymentStatus.Paid || r.PaymentStatus == PaymentStatus.PartiallyPaid))
            .ToListAsync(cancellationToken);

        DateTime Effective(Registration r) => r.PaidAt ?? r.UpdatedAt ?? r.CreatedAt;

        if (customStart.HasValue && customEnd.HasValue && customEnd.Value >= customStart.Value)
        {
            return BuildCustomRangePoints(paidRegistrations, Effective, customStart.Value, customEnd.Value);
        }

        if (string.Equals(range, "quarter", StringComparison.OrdinalIgnoreCase))
        {
            return BuildQuarterlyPoints(paidRegistrations, Effective);
        }

        if (string.Equals(range, "year", StringComparison.OrdinalIgnoreCase))
        {
            return BuildMonthlyPoints(paidRegistrations, Effective, 12, 0);
        }

        int normalizedOffset = Math.Max(0, offset);

        if (string.Equals(range, "day", StringComparison.OrdinalIgnoreCase))
        {
            DateOnly anchorEnd = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-15 * normalizedOffset);
            DateOnly anchorStart = anchorEnd.AddDays(-14);
            return BuildDailyPoints(paidRegistrations, Effective, anchorStart, anchorEnd, "dd MMM");
        }

        if (string.Equals(range, "week", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWeeklyPoints(paidRegistrations, Effective, 8, normalizedOffset);
        }

        return BuildMonthlyPoints(paidRegistrations, Effective, 6, normalizedOffset);
    }

    /// <summary>
    /// Un punto por semana natural (lunes a domingo), de mas antigua a mas reciente.
    /// El bucket mas reciente es la semana actual menos <paramref name="offset"/>
    /// semanas, para que los botones de anterior/siguiente del grafico de Revenue
    /// puedan navegar hacia atras en el tiempo en vez de terminar siempre en hoy.
    /// </summary>
    private static List<RevenuePointDto> BuildWeeklyPoints(List<Registration> paidRegistrations, Func<Registration, DateTime> effective, int weekCount, int offset)
    {
        DateTime today = DateTime.UtcNow.Date;
        int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        DateTime thisWeekStart = today.AddDays(-daysSinceMonday);
        DateTime anchorWeekStart = thisWeekStart.AddDays(-7 * offset);

        List<RevenuePointDto> points = [];

        for (int i = weekCount - 1; i >= 0; i--)
        {
            DateTime weekStart = anchorWeekStart.AddDays(-7 * i);
            DateTime weekEnd = weekStart.AddDays(7);

            decimal amount = paidRegistrations
                .Where(r => effective(r) >= weekStart && effective(r) < weekEnd)
                .Sum(r => r.AmountPaid);

            points.Add(new RevenuePointDto
            {
                Label = weekStart.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture),
                Amount = amount
            });
        }

        return points;
    }

    /// <summary>
    /// Un punto por mes natural, de mas antiguo a mas reciente. El bucket mas
    /// reciente es el mes actual menos <paramref name="offset"/> meses (0 = este
    /// mes), asi que tanto la vista fija de 12 meses ("year") como la vista
    /// navegable de 6 en 6 meses ("month") comparten el mismo helper.
    /// </summary>
    private static List<RevenuePointDto> BuildMonthlyPoints(List<Registration> paidRegistrations, Func<Registration, DateTime> effective, int monthCount, int offset)
    {
        List<RevenuePointDto> points = [];

        DateTime anchorMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-offset);

        for (int i = monthCount - 1; i >= 0; i--)
        {
            DateTime monthStart = anchorMonthStart.AddMonths(-i);
            DateTime monthEnd = monthStart.AddMonths(1);

            decimal amount = paidRegistrations
                .Where(r => effective(r) >= monthStart && effective(r) < monthEnd)
                .Sum(r => r.AmountPaid);

            points.Add(new RevenuePointDto
            {
                Label = monthStart.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                Amount = amount
            });
        }

        return points;
    }

    private static List<RevenuePointDto> BuildQuarterlyPoints(List<Registration> paidRegistrations, Func<Registration, DateTime> effective)
    {
        List<RevenuePointDto> points = [];

        DateTime currentQuarterStart = new DateTime(DateTime.UtcNow.Year, (((DateTime.UtcNow.Month - 1) / 3) * 3) + 1, 1);

        for (int i = 3; i >= 0; i--)
        {
            DateTime quarterStart = currentQuarterStart.AddMonths(-3 * i);
            DateTime quarterEnd = quarterStart.AddMonths(3);
            int quarterNumber = ((quarterStart.Month - 1) / 3) + 1;

            decimal amount = paidRegistrations
                .Where(r => effective(r) >= quarterStart && effective(r) < quarterEnd)
                .Sum(r => r.AmountPaid);

            points.Add(new RevenuePointDto
            {
                Label = $"Q{quarterNumber} {quarterStart.Year}",
                Amount = amount
            });
        }

        return points;
    }

    private static List<RevenuePointDto> BuildDailyPoints(List<Registration> paidRegistrations, Func<Registration, DateTime> effective, DateOnly startDay, DateOnly endDay, string labelFormat)
    {
        List<RevenuePointDto> points = [];

        for (DateOnly day = startDay; day <= endDay; day = day.AddDays(1))
        {
            decimal amount = paidRegistrations
                .Where(r => DateOnly.FromDateTime(effective(r)) == day)
                .Sum(r => r.AmountPaid);

            points.Add(new RevenuePointDto { Label = day.ToString(labelFormat, System.Globalization.CultureInfo.InvariantCulture), Amount = amount });
        }

        return points;
    }

    private static List<RevenuePointDto> BuildCustomRangePoints(List<Registration> paidRegistrations, Func<Registration, DateTime> effective, DateOnly start, DateOnly end)
    {
        int spanDays = end.DayNumber - start.DayNumber;

        if (spanDays <= 31)
        {
            return BuildDailyPoints(paidRegistrations, effective, start, end, "dd/MM");
        }

        List<RevenuePointDto> points = [];
        DateTime monthCursor = new DateTime(start.Year, start.Month, 1);
        DateTime endExclusive = new DateTime(end.Year, end.Month, 1).AddMonths(1);

        while (monthCursor < endExclusive)
        {
            DateTime monthEnd = monthCursor.AddMonths(1);

            decimal amount = paidRegistrations
                .Where(r => effective(r) >= monthCursor && effective(r) < monthEnd)
                .Sum(r => r.AmountPaid);

            points.Add(new RevenuePointDto
            {
                Label = monthCursor.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                Amount = amount
            });

            monthCursor = monthEnd;
        }

        return points;
    }
}
