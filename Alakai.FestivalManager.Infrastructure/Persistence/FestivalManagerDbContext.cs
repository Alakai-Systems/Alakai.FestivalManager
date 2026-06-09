using Alakai.FestivalManager.Application.Interfaces;

namespace Alakai.FestivalManager.Infrastructure.Persistence;

public class FestivalManagerDbContext : DbContext, IApplicationDbContext
{
    public FestivalManagerDbContext(DbContextOptions<FestivalManagerDbContext> options)
        : base(options)
    {
    }
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<Edition> Editions => Set<Edition>();
    public DbSet<PassType> PassTypes => Set<PassType>();
    public DbSet<Level> Levels => Set<Level>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FestivalManagerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}