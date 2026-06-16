param(
    [string]$SolutionRoot = "."
)

$ErrorActionPreference = "Stop"

function New-FileIfNotExists {
    param(
        [string]$Path,
        [string]$Content
    )

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

function Add-TextIfMissing {
    param(
        [string]$Path,
        [string]$SearchText,
        [string]$InsertText,
        [string]$AnchorText
    )

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped patch. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($SearchText)) {
        Write-Host "Patch already present: $SearchText"
        return
    }

    if ($content.Contains($AnchorText)) {
        $content = $content.Replace($AnchorText, "$AnchorText`r`n$InsertText")
        Set-Content -Path $Path -Value $content -Encoding UTF8
        Write-Host "Patched: $Path"
    }
    else {
        Write-Host "Skipped patch. Anchor not found in: $Path"
    }
}

$root = Resolve-Path $SolutionRoot

# ============================================================
# DOMAIN
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\CompetitionFormat.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum CompetitionFormat
{
    Solo = 1,
    Couple = 2,
    Team = 3
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\CompetitionEntryStatus.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum CompetitionEntryStatus
{
    Registered = 1,
    Confirmed = 2,
    WaitingPartner = 3,
    Cancelled = 4
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Entities\Competition.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class Competition : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CompetitionFormat Format { get; set; }

    public int? MaxParticipants { get; set; }
    public int? MaxTeamSize { get; set; }

    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }

    public decimal Price { get; set; }

    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CompetitionEntry> Entries { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Entities\CompetitionEntry.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class CompetitionEntry : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = default!;

    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public Guid? PartnerRegistrationId { get; set; }
    public Registration? PartnerRegistration { get; set; }

    public DanceRole? DanceRole { get; set; }

    public string? TeamName { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public CompetitionEntryStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;
}
'@

# ============================================================
# INFRASTRUCTURE CONFIGURATIONS
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Persistence\Configurations\CompetitionConfiguration.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competitions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.EditionId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Format)
            .IsRequired();

        builder.Property(c => c.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.RequiresPartner)
            .IsRequired();

        builder.Property(c => c.RequiresRole)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => c.EditionId);

        builder.HasIndex(c => new { c.EditionId, c.Name })
            .IsUnique();

        builder.HasOne(c => c.Edition)
            .WithMany()
            .HasForeignKey(c => c.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Persistence\Configurations\CompetitionEntryConfiguration.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class CompetitionEntryConfiguration : IEntityTypeConfiguration<CompetitionEntry>
{
    public void Configure(EntityTypeBuilder<CompetitionEntry> builder)
    {
        builder.ToTable("CompetitionEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CompetitionId)
            .IsRequired();

        builder.Property(e => e.RegistrationId)
            .IsRequired();

        builder.Property(e => e.TeamName)
            .HasMaxLength(150);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CancelledAt);

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasIndex(e => e.CompetitionId);

        builder.HasIndex(e => e.RegistrationId);

        builder.HasIndex(e => e.PartnerRegistrationId);

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => new { e.CompetitionId, e.RegistrationId })
            .IsUnique();

        builder.HasOne(e => e.Competition)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Registration)
            .WithMany()
            .HasForeignKey(e => e.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PartnerRegistration)
            .WithMany()
            .HasForeignKey(e => e.PartnerRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
'@

# ============================================================
# APPLICATION REPOSITORY INTERFACES
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Contracts\Repositories\ICompetitionRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionRepository
{
    Task AddAsync(Competition competition, CancellationToken cancellationToken = default);
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Competition>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default);
    void Update(Competition competition);
    void Delete(Competition competition);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Contracts\Repositories\ICompetitionEntryRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionEntryRepository
{
    Task AddAsync(CompetitionEntry entry, CancellationToken cancellationToken = default);
    Task<CompetitionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCompetitionAndRegistrationAsync(Guid competitionId, Guid registrationId, CancellationToken cancellationToken = default);
    void Update(CompetitionEntry entry);
    void Delete(CompetitionEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

# ============================================================
# INFRASTRUCTURE REPOSITORIES
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Repositories\CompetitionRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionRepository : ICompetitionRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Competition competition, CancellationToken cancellationToken = default)
    {
        await _context.Competitions.AddAsync(competition, cancellationToken);
    }

    public async Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Competitions.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Competition>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions.Where(c => c.EditionId == editionId).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions.AnyAsync(c => c.EditionId == editionId && c.Name == name, cancellationToken);
    }

    public void Update(Competition competition)
    {
        _context.Competitions.Update(competition);
    }

    public void Delete(Competition competition)
    {
        _context.Competitions.Remove(competition);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Repositories\CompetitionEntryRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionEntryRepository : ICompetitionEntryRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionEntryRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompetitionEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionEntries.AddAsync(entry, cancellationToken);
    }

    public async Task<CompetitionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.Where(e => e.CompetitionId == competitionId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.Where(e => e.RegistrationId == registrationId).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCompetitionAndRegistrationAsync(Guid competitionId, Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.AnyAsync(e => e.CompetitionId == competitionId && e.RegistrationId == registrationId, cancellationToken);
    }

    public void Update(CompetitionEntry entry)
    {
        _context.CompetitionEntries.Update(entry);
    }

    public void Delete(CompetitionEntry entry)
    {
        _context.CompetitionEntries.Remove(entry);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

# ============================================================
# COMPETITIONS APPLICATION
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\DTOs\CompetitionDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.DTOs;

public class CompetitionDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompetitionFormat Format { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MaxTeamSize { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public decimal Price { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\CreateCompetition\CreateCompetitionCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompetitionFormat Format { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MaxTeamSize { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public decimal Price { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public int SortOrder { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\UpdateCompetition\UpdateCompetitionCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionCommand
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompetitionFormat Format { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MaxTeamSize { get; set; }
    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }
    public decimal Price { get; set; }
    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\DeleteCompetition\DeleteCompetitionCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.DeleteCompetition;

public class DeleteCompetitionCommand
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Queries\GetCompetitionById\GetCompetitionByIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionById;

public class GetCompetitionByIdQuery
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Queries\GetCompetitionsByEditionId\GetCompetitionsByEditionIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionsByEditionId;

public class GetCompetitionsByEditionIdQuery
{
    public Guid EditionId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\CreateCompetitionResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class CreateCompetitionResponse
{
    public CompetitionDto Competition { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\UpdateCompetitionResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class UpdateCompetitionResponse
{
    public CompetitionDto Competition { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\DeleteCompetitionResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class DeleteCompetitionResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\GetCompetitionByIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class GetCompetitionByIdResponse
{
    public CompetitionDto Competition { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\GetCompetitionsResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class GetCompetitionsResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Responses\GetCompetitionsByEditionIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Responses;

public class GetCompetitionsByEditionIdResponse
{
    public IReadOnlyList<CompetitionDto> Competitions { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Mappings\CompetitionMappingProfile.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Mappings;

public class CompetitionMappingProfile : Profile
{
    public CompetitionMappingProfile()
    {
        CreateMap<Competition, CompetitionDto>();

        CreateMap<CreateCompetitionCommand, Competition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateCompetitionCommand, Competition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Validators\CreateCompetitionCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Validators;

public class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        RuleFor(c => c.EditionId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
        RuleFor(c => c.Description).MaximumLength(1000);
        RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MaxParticipants).GreaterThan(0).When(c => c.MaxParticipants.HasValue);
        RuleFor(c => c.MaxTeamSize).GreaterThan(0).When(c => c.MaxTeamSize.HasValue);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Validators\UpdateCompetitionCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Validators;

public class UpdateCompetitionCommandValidator : AbstractValidator<UpdateCompetitionCommand>
{
    public UpdateCompetitionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.EditionId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
        RuleFor(c => c.Description).MaximumLength(1000);
        RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MaxParticipants).GreaterThan(0).When(c => c.MaxParticipants.HasValue);
        RuleFor(c => c.MaxTeamSize).GreaterThan(0).When(c => c.MaxTeamSize.HasValue);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\CreateCompetition\CreateCompetitionHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionDto> HandleAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _competitionRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A competition named '{command.Name}' already exists for this edition.");
        }

        Competition competition = _mapper.Map<Competition>(command);
        competition.CreatedAt = DateTime.UtcNow;
        competition.IsActive = true;

        await _competitionRepository.AddAsync(competition, cancellationToken);
        await _competitionRepository.SaveChangesAsync(cancellationToken);

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Queries\GetCompetitionById\GetCompetitionByIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionById;

public class GetCompetitionByIdHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionByIdHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionDto> HandleAsync(GetCompetitionByIdQuery query, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(query.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{query.Id}' was not found.");
        }

        return _mapper.Map<CompetitionDto>(competition);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Queries\GetCompetitions\GetCompetitionsHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitions;

public class GetCompetitionsHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionsHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Competition> competitions = await _competitionRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionDto>>(competitions);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Queries\GetCompetitionsByEditionId\GetCompetitionsByEditionIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionsByEditionId;

public class GetCompetitionsByEditionIdHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionsByEditionIdHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionDto>> HandleAsync(GetCompetitionsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Competition> competitions = await _competitionRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionDto>>(competitions);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\UpdateCompetition\UpdateCompetitionHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionDto> HandleAsync(UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, competition);
        competition.UpdatedAt = DateTime.UtcNow;

        _competitionRepository.Update(competition);
        await _competitionRepository.SaveChangesAsync(cancellationToken);

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Commands\DeleteCompetition\DeleteCompetitionHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.DeleteCompetition;

public class DeleteCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;

    public DeleteCompetitionHandler(ICompetitionRepository competitionRepository)
    {
        _competitionRepository = competitionRepository;
    }

    public async Task HandleAsync(DeleteCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");
        }

        competition.IsActive = false;
        competition.UpdatedAt = DateTime.UtcNow;

        _competitionRepository.Update(competition);
        await _competitionRepository.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Services\ICompetitionService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Services;

public interface ICompetitionService
{
    Task<ApiResponse<CreateCompetitionResponse>> CreateAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateCompetitionResponse>> UpdateAsync(Guid id, UpdateCompetitionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteCompetitionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\Competitions\Services\CompetitionService.cs" @'
namespace Alakai.FestivalManager.Application.Features.Competitions.Services;

public class CompetitionService : ICompetitionService
{
    private readonly CreateCompetitionHandler _createCompetitionHandler;
    private readonly GetCompetitionByIdHandler _getCompetitionByIdHandler;
    private readonly GetCompetitionsHandler _getCompetitionsHandler;
    private readonly GetCompetitionsByEditionIdHandler _getCompetitionsByEditionIdHandler;
    private readonly UpdateCompetitionHandler _updateCompetitionHandler;
    private readonly DeleteCompetitionHandler _deleteCompetitionHandler;
    private readonly IValidator<CreateCompetitionCommand> _createCompetitionValidator;
    private readonly IValidator<UpdateCompetitionCommand> _updateCompetitionValidator;

    public CompetitionService(CreateCompetitionHandler createCompetitionHandler, GetCompetitionByIdHandler getCompetitionByIdHandler, GetCompetitionsHandler getCompetitionsHandler, GetCompetitionsByEditionIdHandler getCompetitionsByEditionIdHandler, UpdateCompetitionHandler updateCompetitionHandler, DeleteCompetitionHandler deleteCompetitionHandler, IValidator<CreateCompetitionCommand> createCompetitionValidator, IValidator<UpdateCompetitionCommand> updateCompetitionValidator)
    {
        _createCompetitionHandler = createCompetitionHandler;
        _getCompetitionByIdHandler = getCompetitionByIdHandler;
        _getCompetitionsHandler = getCompetitionsHandler;
        _getCompetitionsByEditionIdHandler = getCompetitionsByEditionIdHandler;
        _updateCompetitionHandler = updateCompetitionHandler;
        _deleteCompetitionHandler = deleteCompetitionHandler;
        _createCompetitionValidator = createCompetitionValidator;
        _updateCompetitionValidator = updateCompetitionValidator;
    }

    public async Task<ApiResponse<CreateCompetitionResponse>> CreateAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createCompetitionValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<CreateCompetitionResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        CompetitionDto competitionDto = await _createCompetitionHandler.HandleAsync(command, cancellationToken);

        return ApiResponse<CreateCompetitionResponse>.SuccessResponse(new CreateCompetitionResponse { Competition = competitionDto }, $"{competitionDto.Name} is correctly registered");
    }

    public async Task<ApiResponse<GetCompetitionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        CompetitionDto competitionDto = await _getCompetitionByIdHandler.HandleAsync(new GetCompetitionByIdQuery { Id = id }, cancellationToken);

        return ApiResponse<GetCompetitionByIdResponse>.SuccessResponse(new GetCompetitionByIdResponse { Competition = competitionDto }, $"{competitionDto.Name} retrieved successfully");
    }

    public async Task<ApiResponse<GetCompetitionsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionDto> competitions = await _getCompetitionsHandler.HandleAsync(cancellationToken);

        return ApiResponse<GetCompetitionsResponse>.SuccessResponse(new GetCompetitionsResponse { Competitions = competitions }, $"There are {competitions.Count} competitions registered");
    }

    public async Task<ApiResponse<GetCompetitionsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionDto> competitions = await _getCompetitionsByEditionIdHandler.HandleAsync(new GetCompetitionsByEditionIdQuery { EditionId = editionId }, cancellationToken);

        return ApiResponse<GetCompetitionsByEditionIdResponse>.SuccessResponse(new GetCompetitionsByEditionIdResponse { Competitions = competitions }, $"There are {competitions.Count} competitions for this edition");
    }

    public async Task<ApiResponse<UpdateCompetitionResponse>> UpdateAsync(Guid id, UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        ValidationResult validationResult = await _updateCompetitionValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<UpdateCompetitionResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        CompetitionDto competitionDto = await _updateCompetitionHandler.HandleAsync(command, cancellationToken);

        return ApiResponse<UpdateCompetitionResponse>.SuccessResponse(new UpdateCompetitionResponse { Competition = competitionDto }, $"{competitionDto.Name} updated successfully");
    }

    public async Task<ApiResponse<DeleteCompetitionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteCompetitionHandler.HandleAsync(new DeleteCompetitionCommand { Id = id }, cancellationToken);

        return ApiResponse<DeleteCompetitionResponse>.SuccessResponse(new DeleteCompetitionResponse { Id = id, Deleted = true }, "Competition deleted successfully");
    }
}
'@

# ============================================================
# COMPETITION ENTRIES APPLICATION
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\DTOs\CompetitionEntryDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.DTOs;

public class CompetitionEntryDto
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? TeamName { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public CompetitionEntryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\CreateCompetitionEntry\CreateCompetitionEntryCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.CreateCompetitionEntry;

public class CreateCompetitionEntryCommand
{
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? TeamName { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\UpdateCompetitionEntry\UpdateCompetitionEntryCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.UpdateCompetitionEntry;

public class UpdateCompetitionEntryCommand
{
    public Guid Id { get; set; }
    public Guid CompetitionId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? TeamName { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public CompetitionEntryStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\DeleteCompetitionEntry\DeleteCompetitionEntryCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.DeleteCompetitionEntry;

public class DeleteCompetitionEntryCommand
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntryById\GetCompetitionEntryByIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntryById;

public class GetCompetitionEntryByIdQuery
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntriesByCompetitionId\GetCompetitionEntriesByCompetitionIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByCompetitionId;

public class GetCompetitionEntriesByCompetitionIdQuery
{
    public Guid CompetitionId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntriesByRegistrationId\GetCompetitionEntriesByRegistrationIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByRegistrationId;

public class GetCompetitionEntriesByRegistrationIdQuery
{
    public Guid RegistrationId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\CreateCompetitionEntryResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class CreateCompetitionEntryResponse
{
    public CompetitionEntryDto CompetitionEntry { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\UpdateCompetitionEntryResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class UpdateCompetitionEntryResponse
{
    public CompetitionEntryDto CompetitionEntry { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\DeleteCompetitionEntryResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class DeleteCompetitionEntryResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\GetCompetitionEntryByIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntryByIdResponse
{
    public CompetitionEntryDto CompetitionEntry { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\GetCompetitionEntriesResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\GetCompetitionEntriesByCompetitionIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesByCompetitionIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Responses\GetCompetitionEntriesByRegistrationIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Responses;

public class GetCompetitionEntriesByRegistrationIdResponse
{
    public IReadOnlyList<CompetitionEntryDto> CompetitionEntries { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Mappings\CompetitionEntryMappingProfile.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Mappings;

public class CompetitionEntryMappingProfile : Profile
{
    public CompetitionEntryMappingProfile()
    {
        CreateMap<CompetitionEntry, CompetitionEntryDto>();

        CreateMap<CreateCompetitionEntryCommand, CompetitionEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateCompetitionEntryCommand, CompetitionEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Validators\CreateCompetitionEntryCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Validators;

public class CreateCompetitionEntryCommandValidator : AbstractValidator<CreateCompetitionEntryCommand>
{
    public CreateCompetitionEntryCommandValidator()
    {
        RuleFor(e => e.CompetitionId).NotEmpty();
        RuleFor(e => e.RegistrationId).NotEmpty();
        RuleFor(e => e.TeamName).MaximumLength(150);
        RuleFor(e => e.Notes).MaximumLength(2000);
        RuleFor(e => e.InternalNotes).MaximumLength(2000);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Validators\UpdateCompetitionEntryCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Validators;

public class UpdateCompetitionEntryCommandValidator : AbstractValidator<UpdateCompetitionEntryCommand>
{
    public UpdateCompetitionEntryCommandValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.CompetitionId).NotEmpty();
        RuleFor(e => e.RegistrationId).NotEmpty();
        RuleFor(e => e.TeamName).MaximumLength(150);
        RuleFor(e => e.Notes).MaximumLength(2000);
        RuleFor(e => e.InternalNotes).MaximumLength(2000);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\CreateCompetitionEntry\CreateCompetitionEntryHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.CreateCompetitionEntry;

public class CreateCompetitionEntryHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionEntryHandler(ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository, IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionEntryDto> HandleAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.CompetitionId, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.CompetitionId}' was not found.");
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{command.RegistrationId}' was not found.");
        }

        if (command.PartnerRegistrationId.HasValue)
        {
            Registration? partnerRegistration = await _registrationRepository.GetByIdAsync(command.PartnerRegistrationId.Value, cancellationToken);

            if (partnerRegistration is null)
            {
                throw new NotFoundException($"Partner registration with id '{command.PartnerRegistrationId}' was not found.");
            }
        }

        bool exists = await _competitionEntryRepository.ExistsByCompetitionAndRegistrationAsync(command.CompetitionId, command.RegistrationId, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException("This registration is already entered in this competition.");
        }

        CompetitionEntry entry = _mapper.Map<CompetitionEntry>(command);
        entry.Status = command.PartnerRegistrationId.HasValue || !competition.RequiresPartner ? CompetitionEntryStatus.Registered : CompetitionEntryStatus.WaitingPartner;
        entry.CreatedAt = DateTime.UtcNow;
        entry.IsActive = true;

        await _competitionEntryRepository.AddAsync(entry, cancellationToken);
        await _competitionEntryRepository.SaveChangesAsync(cancellationToken);

        CompetitionEntryDto dto = _mapper.Map<CompetitionEntryDto>(entry);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntryById\GetCompetitionEntryByIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntryById;

public class GetCompetitionEntryByIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntryByIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionEntryDto> HandleAsync(GetCompetitionEntryByIdQuery query, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? entry = await _competitionEntryRepository.GetByIdAsync(query.Id, cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException($"Competition entry with id '{query.Id}' was not found.");
        }

        return _mapper.Map<CompetitionEntryDto>(entry);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntries\GetCompetitionEntriesHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntries;

public class GetCompetitionEntriesHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntriesByCompetitionId\GetCompetitionEntriesByCompetitionIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByCompetitionId;

public class GetCompetitionEntriesByCompetitionIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesByCompetitionIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(GetCompetitionEntriesByCompetitionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByCompetitionIdAsync(query.CompetitionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Queries\GetCompetitionEntriesByRegistrationId\GetCompetitionEntriesByRegistrationIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByRegistrationId;

public class GetCompetitionEntriesByRegistrationIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesByRegistrationIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(GetCompetitionEntriesByRegistrationIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByRegistrationIdAsync(query.RegistrationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\UpdateCompetitionEntry\UpdateCompetitionEntryHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.UpdateCompetitionEntry;

public class UpdateCompetitionEntryHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionEntryHandler(ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository, IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionEntryDto> HandleAsync(UpdateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? entry = await _competitionEntryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException($"Competition entry with id '{command.Id}' was not found.");
        }

        Competition? competition = await _competitionRepository.GetByIdAsync(command.CompetitionId, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.CompetitionId}' was not found.");
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{command.RegistrationId}' was not found.");
        }

        if (command.PartnerRegistrationId.HasValue)
        {
            Registration? partnerRegistration = await _registrationRepository.GetByIdAsync(command.PartnerRegistrationId.Value, cancellationToken);

            if (partnerRegistration is null)
            {
                throw new NotFoundException($"Partner registration with id '{command.PartnerRegistrationId}' was not found.");
            }
        }

        _mapper.Map(command, entry);
        entry.UpdatedAt = DateTime.UtcNow;

        _competitionEntryRepository.Update(entry);
        await _competitionEntryRepository.SaveChangesAsync(cancellationToken);

        CompetitionEntryDto dto = _mapper.Map<CompetitionEntryDto>(entry);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Commands\DeleteCompetitionEntry\DeleteCompetitionEntryHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.DeleteCompetitionEntry;

public class DeleteCompetitionEntryHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;

    public DeleteCompetitionEntryHandler(ICompetitionEntryRepository competitionEntryRepository)
    {
        _competitionEntryRepository = competitionEntryRepository;
    }

    public async Task HandleAsync(DeleteCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? entry = await _competitionEntryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException($"Competition entry with id '{command.Id}' was not found.");
        }

        entry.IsActive = false;
        entry.Status = CompetitionEntryStatus.Cancelled;
        entry.CancelledAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        _competitionEntryRepository.Update(entry);
        await _competitionEntryRepository.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Services\ICompetitionEntryService.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Services;

public interface ICompetitionEntryService
{
    Task<ApiResponse<CreateCompetitionEntryResponse>> CreateAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntryByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateCompetitionEntryResponse>> UpdateAsync(Guid id, UpdateCompetitionEntryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteCompetitionEntryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\CompetitionEntries\Services\CompetitionEntryService.cs" @'
namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Services;

public class CompetitionEntryService : ICompetitionEntryService
{
    private readonly CreateCompetitionEntryHandler _createCompetitionEntryHandler;
    private readonly GetCompetitionEntryByIdHandler _getCompetitionEntryByIdHandler;
    private readonly GetCompetitionEntriesHandler _getCompetitionEntriesHandler;
    private readonly GetCompetitionEntriesByCompetitionIdHandler _getCompetitionEntriesByCompetitionIdHandler;
    private readonly GetCompetitionEntriesByRegistrationIdHandler _getCompetitionEntriesByRegistrationIdHandler;
    private readonly UpdateCompetitionEntryHandler _updateCompetitionEntryHandler;
    private readonly DeleteCompetitionEntryHandler _deleteCompetitionEntryHandler;
    private readonly IValidator<CreateCompetitionEntryCommand> _createCompetitionEntryValidator;
    private readonly IValidator<UpdateCompetitionEntryCommand> _updateCompetitionEntryValidator;

    public CompetitionEntryService(CreateCompetitionEntryHandler createCompetitionEntryHandler, GetCompetitionEntryByIdHandler getCompetitionEntryByIdHandler, GetCompetitionEntriesHandler getCompetitionEntriesHandler, GetCompetitionEntriesByCompetitionIdHandler getCompetitionEntriesByCompetitionIdHandler, GetCompetitionEntriesByRegistrationIdHandler getCompetitionEntriesByRegistrationIdHandler, UpdateCompetitionEntryHandler updateCompetitionEntryHandler, DeleteCompetitionEntryHandler deleteCompetitionEntryHandler, IValidator<CreateCompetitionEntryCommand> createCompetitionEntryValidator, IValidator<UpdateCompetitionEntryCommand> updateCompetitionEntryValidator)
    {
        _createCompetitionEntryHandler = createCompetitionEntryHandler;
        _getCompetitionEntryByIdHandler = getCompetitionEntryByIdHandler;
        _getCompetitionEntriesHandler = getCompetitionEntriesHandler;
        _getCompetitionEntriesByCompetitionIdHandler = getCompetitionEntriesByCompetitionIdHandler;
        _getCompetitionEntriesByRegistrationIdHandler = getCompetitionEntriesByRegistrationIdHandler;
        _updateCompetitionEntryHandler = updateCompetitionEntryHandler;
        _deleteCompetitionEntryHandler = deleteCompetitionEntryHandler;
        _createCompetitionEntryValidator = createCompetitionEntryValidator;
        _updateCompetitionEntryValidator = updateCompetitionEntryValidator;
    }

    public async Task<ApiResponse<CreateCompetitionEntryResponse>> CreateAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createCompetitionEntryValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<CreateCompetitionEntryResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        CompetitionEntryDto competitionEntryDto = await _createCompetitionEntryHandler.HandleAsync(command, cancellationToken);

        return ApiResponse<CreateCompetitionEntryResponse>.SuccessResponse(new CreateCompetitionEntryResponse { CompetitionEntry = competitionEntryDto }, "Competition entry registered successfully");
    }

    public async Task<ApiResponse<GetCompetitionEntryByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        CompetitionEntryDto competitionEntryDto = await _getCompetitionEntryByIdHandler.HandleAsync(new GetCompetitionEntryByIdQuery { Id = id }, cancellationToken);

        return ApiResponse<GetCompetitionEntryByIdResponse>.SuccessResponse(new GetCompetitionEntryByIdResponse { CompetitionEntry = competitionEntryDto }, "Competition entry retrieved successfully");
    }

    public async Task<ApiResponse<GetCompetitionEntriesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesHandler.HandleAsync(cancellationToken);

        return ApiResponse<GetCompetitionEntriesResponse>.SuccessResponse(new GetCompetitionEntriesResponse { CompetitionEntries = competitionEntries }, $"There are {competitionEntries.Count} competition entries registered");
    }

    public async Task<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesByCompetitionIdHandler.HandleAsync(new GetCompetitionEntriesByCompetitionIdQuery { CompetitionId = competitionId }, cancellationToken);

        return ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>.SuccessResponse(new GetCompetitionEntriesByCompetitionIdResponse { CompetitionEntries = competitionEntries }, $"There are {competitionEntries.Count} entries for this competition");
    }

    public async Task<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesByRegistrationIdHandler.HandleAsync(new GetCompetitionEntriesByRegistrationIdQuery { RegistrationId = registrationId }, cancellationToken);

        return ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>.SuccessResponse(new GetCompetitionEntriesByRegistrationIdResponse { CompetitionEntries = competitionEntries }, $"There are {competitionEntries.Count} entries for this registration");
    }

    public async Task<ApiResponse<UpdateCompetitionEntryResponse>> UpdateAsync(Guid id, UpdateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        ValidationResult validationResult = await _updateCompetitionEntryValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<UpdateCompetitionEntryResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        CompetitionEntryDto competitionEntryDto = await _updateCompetitionEntryHandler.HandleAsync(command, cancellationToken);

        return ApiResponse<UpdateCompetitionEntryResponse>.SuccessResponse(new UpdateCompetitionEntryResponse { CompetitionEntry = competitionEntryDto }, "Competition entry updated successfully");
    }

    public async Task<ApiResponse<DeleteCompetitionEntryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteCompetitionEntryHandler.HandleAsync(new DeleteCompetitionEntryCommand { Id = id }, cancellationToken);

        return ApiResponse<DeleteCompetitionEntryResponse>.SuccessResponse(new DeleteCompetitionEntryResponse { Id = id, Deleted = true }, "Competition entry deleted successfully");
    }
}
'@

# ============================================================
# API CONTROLLERS
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Api\Controllers\CompetitionsController.cs" @'
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/competitions")]
public class CompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;

    public CompetitionsController(ICompetitionService competitionService)
    {
        _competitionService = competitionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetCompetitionsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionsResponse> response = await _competitionService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionByIdResponse> response = await _competitionService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionsByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionsByEditionIdResponse> response = await _competitionService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateCompetitionResponse>>> Create([FromBody] CreateCompetitionCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<CreateCompetitionResponse> response = await _competitionService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateCompetitionResponse>>> Update(Guid id, [FromBody] UpdateCompetitionCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateCompetitionResponse> response = await _competitionService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteCompetitionResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteCompetitionResponse> response = await _competitionService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Api\Controllers\CompetitionEntriesController.cs" @'
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/competition-entries")]
public class CompetitionEntriesController : ControllerBase
{
    private readonly ICompetitionEntryService _competitionEntryService;

    public CompetitionEntriesController(ICompetitionEntryService competitionEntryService)
    {
        _competitionEntryService = competitionEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesResponse> response = await _competitionEntryService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntryByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntryByIdResponse> response = await _competitionEntryService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-competition/{competitionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>>> GetByCompetitionId(Guid competitionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesByCompetitionIdResponse> response = await _competitionEntryService.GetByCompetitionIdAsync(competitionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<ActionResult<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>>> GetByRegistrationId(Guid registrationId, CancellationToken cancellationToken)
    {
        ApiResponse<GetCompetitionEntriesByRegistrationIdResponse> response = await _competitionEntryService.GetByRegistrationIdAsync(registrationId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateCompetitionEntryResponse>>> Create([FromBody] CreateCompetitionEntryCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<CreateCompetitionEntryResponse> response = await _competitionEntryService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateCompetitionEntryResponse>>> Update(Guid id, [FromBody] UpdateCompetitionEntryCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<UpdateCompetitionEntryResponse> response = await _competitionEntryService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteCompetitionEntryResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteCompetitionEntryResponse> response = await _competitionEntryService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
'@

# ============================================================
# PATCH DBCONTEXT DBSETS
# ============================================================

$dbContextPath = "$root\Alakai.FestivalManager.Infrastructure\Persistence\FestivalManagerDbContext.cs"

Add-TextIfMissing $dbContextPath "public DbSet<Competition> Competitions" "    public DbSet<Competition> Competitions => Set<Competition>();`r`n    public DbSet<CompetitionEntry> CompetitionEntries => Set<CompetitionEntry>();" "public DbSet<Registration> Registrations => Set<Registration>();"

New-FileIfNotExists "$root\NEXT_STEPS_COMPETITIONS_FULL.txt" @'
Script completed.

Manual steps still required:
1. Register DI in ApplicationDependencyInjectionExtension:
   services.AddScoped<CreateCompetitionHandler>();
   services.AddScoped<GetCompetitionByIdHandler>();
   services.AddScoped<GetCompetitionsHandler>();
   services.AddScoped<GetCompetitionsByEditionIdHandler>();
   services.AddScoped<UpdateCompetitionHandler>();
   services.AddScoped<DeleteCompetitionHandler>();
   services.AddScoped<ICompetitionService, CompetitionService>();

   services.AddScoped<CreateCompetitionEntryHandler>();
   services.AddScoped<GetCompetitionEntryByIdHandler>();
   services.AddScoped<GetCompetitionEntriesHandler>();
   services.AddScoped<GetCompetitionEntriesByCompetitionIdHandler>();
   services.AddScoped<GetCompetitionEntriesByRegistrationIdHandler>();
   services.AddScoped<UpdateCompetitionEntryHandler>();
   services.AddScoped<DeleteCompetitionEntryHandler>();
   services.AddScoped<ICompetitionEntryService, CompetitionEntryService>();

2. Register DI in InfrastructureDependencyInjectionExtension:
   services.AddScoped<ICompetitionRepository, CompetitionRepository>();
   services.AddScoped<ICompetitionEntryRepository, CompetitionEntryRepository>();

3. Compile. If namespace/global using errors appear, add the same using/global using pattern used by Registrations.

4. Add migration:
   dotnet ef migrations add AddCompetitionsAndCompetitionEntries -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api

5. Update database:
   dotnet ef database update -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api
'@

Write-Host ""
Write-Host "Full Competitions + CompetitionEntries backend generated."
Write-Host "Only DI and migrations remain manual."
