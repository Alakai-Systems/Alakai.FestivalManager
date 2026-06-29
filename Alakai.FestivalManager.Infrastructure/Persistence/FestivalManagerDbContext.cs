namespace Alakai.FestivalManager.Infrastructure.Persistence;

public class FestivalManagerDbContext : DbContext
{
    public FestivalManagerDbContext(DbContextOptions<FestivalManagerDbContext> options)
        : base(options)
    {
    }
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<Edition> Editions => Set<Edition>();
    public DbSet<PassType> PassTypes => Set<PassType>();
    public DbSet<Level> Levels => Set<Level>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<CompetitionEntry> CompetitionEntries => Set<CompetitionEntry>();
    public DbSet<CompetitionCapacity> CompetitionCapacities => Set<CompetitionCapacity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<EmailLayout> EmailLayouts => Set<EmailLayout>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FestivalManagerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}





