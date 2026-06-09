namespace Alakai.FestivalManager.Infrastructure.Persistence;

public class FestivalManagerDbContext : DbContext
{
    public FestivalManagerDbContext(DbContextOptions<FestivalManagerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}