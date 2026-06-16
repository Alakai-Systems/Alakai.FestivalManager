param(
    [string]$SolutionRoot = "."
)

$ErrorActionPreference = "Stop"

function New-FileIfNotExists {
    param([string]$Path, [string]$Content)

    $directory = Split-Path $Path -Parent

    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    if (-not (Test-Path $Path)) {
        Set-Content -Path $Path -Value $Content -Encoding UTF8
        Write-Host "Created: $Path"
    }
    else {
        Write-Host "Skipped existing: $Path"
    }
}

function Insert-BeforeIfMissing {
    param([string]$Path, [string]$SearchText, [string]$InsertText, [string]$AnchorText)

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped insert. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($SearchText)) {
        Write-Host "Already present: $SearchText"
        return
    }

    if ($content.Contains($AnchorText)) {
        $content = $content.Replace($AnchorText, "$InsertText`r`n$AnchorText")
        Set-Content -Path $Path -Value $content -Encoding UTF8
        Write-Host "Patched: $Path"
    }
    else {
        Write-Host "Skipped insert. Anchor not found in: $Path"
    }
}

function Insert-AfterIfMissing {
    param([string]$Path, [string]$SearchText, [string]$InsertText, [string]$AnchorText)

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped insert. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($SearchText)) {
        Write-Host "Already present: $SearchText"
        return
    }

    if ($content.Contains($AnchorText)) {
        $content = $content.Replace($AnchorText, "$AnchorText`r`n$InsertText")
        Set-Content -Path $Path -Value $content -Encoding UTF8
        Write-Host "Patched: $Path"
    }
    else {
        Write-Host "Skipped insert. Anchor not found in: $Path"
    }
}

function Replace-TextIfExists {
    param([string]$Path, [string]$OldText, [string]$NewText)

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped replace. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($OldText)) {
        $content = $content.Replace($OldText, $NewText)
        Set-Content -Path $Path -Value $content -Encoding UTF8
        Write-Host "Patched: $Path"
    }
    else {
        Write-Host "Skipped replace. Text not found in: $Path"
    }
}

$root = Resolve-Path $SolutionRoot

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\MixAndMatchLevel.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum MixAndMatchLevel
{
    Open = 1,
    Advanced = 2
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Entities\CompetitionCapacity.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class CompetitionCapacity : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public MixAndMatchLevel? MixAndMatchLevel { get; set; }

    public DanceRole DanceRole { get; set; }

    public int Capacity { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Persistence\Configurations\CompetitionCapacityConfiguration.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class CompetitionCapacityConfiguration : IEntityTypeConfiguration<CompetitionCapacity>
{
    public void Configure(EntityTypeBuilder<CompetitionCapacity> builder)
    {
        builder.ToTable("CompetitionCapacities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompetitionId)
            .IsRequired();

        builder.Property(c => c.MixAndMatchLevel);

        builder.Property(c => c.DanceRole)
            .IsRequired();

        builder.Property(c => c.Capacity)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => c.CompetitionId);

        builder.HasIndex(c => new { c.CompetitionId, c.MixAndMatchLevel, c.DanceRole })
            .IsUnique();

        builder.HasOne(c => c.Competition)
            .WithMany(c => c.Capacities)
            .HasForeignKey(c => c.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Contracts\Repositories\ICompetitionCapacityRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionCapacityRepository
{
    Task AddAsync(CompetitionCapacity capacity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CompetitionCapacity> capacities, CancellationToken cancellationToken = default);
    Task<CompetitionCapacity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionCapacity>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<CompetitionCapacity?> GetByCompetitionLevelAndRoleAsync(Guid competitionId, MixAndMatchLevel? mixAndMatchLevel, DanceRole danceRole, CancellationToken cancellationToken = default);
    void Update(CompetitionCapacity capacity);
    void Delete(CompetitionCapacity capacity);
    void DeleteRange(IEnumerable<CompetitionCapacity> capacities);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Repositories\CompetitionCapacityRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionCapacityRepository : ICompetitionCapacityRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionCapacityRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompetitionCapacity capacity, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionCapacities.AddAsync(capacity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CompetitionCapacity> capacities, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionCapacities.AddRangeAsync(capacities, cancellationToken);
    }

    public async Task<CompetitionCapacity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionCapacity>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities
            .Where(c => c.CompetitionId == competitionId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<CompetitionCapacity?> GetByCompetitionLevelAndRoleAsync(Guid competitionId, MixAndMatchLevel? mixAndMatchLevel, DanceRole danceRole, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities.FirstOrDefaultAsync(c => c.CompetitionId == competitionId && c.MixAndMatchLevel == mixAndMatchLevel && c.DanceRole == danceRole && c.IsActive, cancellationToken);
    }

    public void Update(CompetitionCapacity capacity)
    {
        _context.CompetitionCapacities.Update(capacity);
    }

    public void Delete(CompetitionCapacity capacity)
    {
        _context.CompetitionCapacities.Remove(capacity);
    }

    public void DeleteRange(IEnumerable<CompetitionCapacity> capacities)
    {
        _context.CompetitionCapacities.RemoveRange(capacities);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\DTOs\CompetitionCapacityDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.DTOs;

public class CompetitionCapacityDto
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\CreateCompetition\CreateCompetitionCapacityCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionCapacityCommand
{
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\UpdateCompetition\UpdateCompetitionCapacityCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionCapacityCommand
{
    public Guid? Id { get; set; }
    public MixAndMatchLevel? MixAndMatchLevel { get; set; }
    public DanceRole DanceRole { get; set; }
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@

$competitionPath = "$root\Alakai.FestivalManager.Domain\Entities\Competition.cs"
Insert-BeforeIfMissing $competitionPath "public ICollection<CompetitionCapacity> Capacities" "    public ICollection<CompetitionCapacity> Capacities { get; set; } = [];" "    public ICollection<CompetitionEntry> Entries { get; set; } = [];"

$entryPath = "$root\Alakai.FestivalManager.Domain\Entities\CompetitionEntry.cs"
Insert-AfterIfMissing $entryPath "public MixAndMatchLevel? MixAndMatchLevel" "    public MixAndMatchLevel? MixAndMatchLevel { get; set; }" "    public DanceRole? DanceRole { get; set; }"

$dtoPath = "$root\Alakai.FestivalManager.Application\Features\Competitions\DTOs\CompetitionDto.cs"
Insert-BeforeIfMissing $dtoPath "public IReadOnlyList<CompetitionCapacityDto> Capacities" "    public IReadOnlyList<CompetitionCapacityDto> Capacities { get; set; } = [];" "    public DateTime CreatedAt { get; set; }"

$createCompetitionCommandPath = "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\CreateCompetition\CreateCompetitionCommand.cs"
Insert-BeforeIfMissing $createCompetitionCommandPath "public IReadOnlyList<CreateCompetitionCapacityCommand> Capacities" "    public IReadOnlyList<CreateCompetitionCapacityCommand> Capacities { get; set; } = [];" "    public int SortOrder { get; set; }"

$updateCompetitionCommandPath = "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\UpdateCompetition\UpdateCompetitionCommand.cs"
Insert-BeforeIfMissing $updateCompetitionCommandPath "public IReadOnlyList<UpdateCompetitionCapacityCommand> Capacities" "    public IReadOnlyList<UpdateCompetitionCapacityCommand> Capacities { get; set; } = [];" "    public bool IsActive { get; set; }"

$entryDtoPath = "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\DTOs\CompetitionEntryDto.cs"
Insert-AfterIfMissing $entryDtoPath "public MixAndMatchLevel? MixAndMatchLevel" "    public MixAndMatchLevel? MixAndMatchLevel { get; set; }" "    public DanceRole? DanceRole { get; set; }"

$createEntryCommandPath = "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\CreateCompetitionEntry\CreateCompetitionEntryCommand.cs"
Insert-AfterIfMissing $createEntryCommandPath "public MixAndMatchLevel? MixAndMatchLevel" "    public MixAndMatchLevel? MixAndMatchLevel { get; set; }" "    public DanceRole? DanceRole { get; set; }"

$updateEntryCommandPath = "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\UpdateCompetitionEntry\UpdateCompetitionEntryCommand.cs"
Insert-AfterIfMissing $updateEntryCommandPath "public MixAndMatchLevel? MixAndMatchLevel" "    public MixAndMatchLevel? MixAndMatchLevel { get; set; }" "    public DanceRole? DanceRole { get; set; }"

$dbContextPath = "$root\Alakai.FestivalManager.Infrastructure\Persistence\FestivalManagerDbContext.cs"
Insert-AfterIfMissing $dbContextPath "public DbSet<CompetitionCapacity> CompetitionCapacities" "    public DbSet<CompetitionCapacity> CompetitionCapacities => Set<CompetitionCapacity>();" "    public DbSet<CompetitionEntry> CompetitionEntries => Set<CompetitionEntry>();"

$mappingPath = "$root\Alakai.FestivalManager.Application\Features\Competitions\Mappings\CompetitionMappingProfile.cs"
Replace-TextIfExists $mappingPath "        CreateMap<Competition, CompetitionDto>();" "        CreateMap<CompetitionCapacity, CompetitionCapacityDto>();`r`n        CreateMap<Competition, CompetitionDto>();"

New-FileIfNotExists "$root\NEXT_STEPS_COMPETITION_CAPACITIES.txt" @'
CompetitionCapacity script completed.

Recommended model:
- Use CompetitionCapacities for both SoloJazz and MixAndMatch.
- Remove MaxParticipants and MaxTeamSize later if you want full consistency.
- SoloJazz capacity can use MixAndMatchLevel = null.
- MixAndMatch capacities must use Open/Advanced + Leader/Follower.

Manual steps:
1. Register DI in InfrastructureDependencyInjectionExtension:
   services.AddScoped<ICompetitionCapacityRepository, CompetitionCapacityRepository>();

2. Update CreateCompetitionHandler:
   - after AddAsync(competition), create CompetitionCapacity rows from command.Capacities.
   - set CompetitionId after competition is tracked/saved if needed.

3. Update UpdateCompetitionHandler:
   - sync existing capacities or replace active capacities.

4. Update CreateCompetitionEntryHandler:
   - MixAndMatch requires MixAndMatchLevel and DanceRole.
   - Check capacity by CompetitionId + MixAndMatchLevel + DanceRole.
   - Count active entries for same CompetitionId + MixAndMatchLevel + DanceRole.
   - Throw BusinessRuleException if full.

5. Migration:
   dotnet ef migrations add AddCompetitionCapacities -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api

6. Update database:
   dotnet ef database update -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api
'@

Write-Host ""
Write-Host "CompetitionCapacity backend files generated."
Write-Host "Manual handler logic and DI remain."
