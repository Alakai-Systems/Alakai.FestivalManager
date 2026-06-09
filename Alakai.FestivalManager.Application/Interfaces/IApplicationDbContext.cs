namespace Alakai.FestivalManager.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Festival> Festivals { get; }
    DbSet<Edition> Editions { get; }
    DbSet<PassType> PassTypes { get; }
    DbSet<Level> Levels { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
