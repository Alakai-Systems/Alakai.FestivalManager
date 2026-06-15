using Microsoft.EntityFrameworkCore;
namespace Alakai.FestivalManager.Infrastructure.Persistence;

public class FestivalManagerDbContext : DbContext
{
        public DbSet<Alakai.FestivalManager.Domain.Entities.Registration> Registrations { get; set; } = default!;

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
            modelBuilder.Entity<Alakai.FestivalManager.Domain.Entities.Registration>(entity =>
            {
                entity.ToTable("Registrations");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);

                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinalPrice).HasColumnType("decimal(18,2)");

                entity.Property(e => e.PaymentReference).HasMaxLength(200);
                entity.Property(e => e.DiscountCode).HasMaxLength(100);

                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.InternalNotes).HasMaxLength(2000);

                entity.HasIndex(e => e.EditionId);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => new { e.EditionId, e.Email }).IsUnique(false);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PartnerEmail);

                entity.HasOne(e => e.Edition).WithMany().HasForeignKey(e => e.EditionId).OnDelete(DeleteBehavior.Restrict).IsRequired();
                entity.HasOne(e => e.PassType).WithMany().HasForeignKey(e => e.PassTypeId).OnDelete(DeleteBehavior.Restrict).IsRequired();
                entity.HasOne(e => e.Level).WithMany().HasForeignKey(e => e.LevelId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.PartnerRegistration).WithMany().HasForeignKey(e => e.PartnerRegistrationId).OnDelete(DeleteBehavior.Restrict);
            });
    }
}
