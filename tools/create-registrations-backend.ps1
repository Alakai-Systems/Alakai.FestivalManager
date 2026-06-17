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

function Add-LineIfMissing {
    param([string]$Path, [string]$Line)

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped global using. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if (-not $content.Contains($Line)) {
        Add-Content -Path $Path -Value $Line -Encoding UTF8
        Write-Host "Added global using: $Line"
    }
    else {
        Write-Host "Global using already present: $Line"
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

$root = Resolve-Path $SolutionRoot

# ============================================================
# GLOBAL USINGS
# ============================================================

Add-LineIfMissing "$root\Alakai.FestivalManager.Domain\GlobalUsings.cs" "global using Alakai.FestivalManager.Domain.Enums;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.UpdateDiscountCode;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.DeleteDiscountCode;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodeById;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodes;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodesByEditionId;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.DTOs;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Requests;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Application\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Services;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Api\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Api\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.UpdateDiscountCode;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Api\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Requests;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Api\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;"
Add-LineIfMissing "$root\Alakai.FestivalManager.Api\GlobalUsings.cs" "global using Alakai.FestivalManager.Application.Features.DiscountCodes.Services;"

# ============================================================
# DOMAIN ENUMS
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\DiscountType.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum DiscountType
{
    FixedAmount = 1,
    Percentage = 2
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\DiscountActivationType.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum DiscountActivationType
{
    Immediate = 1,
    AfterThreshold = 2
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\DiscountApplicationStatus.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum DiscountApplicationStatus
{
    None = 1,
    PendingThreshold = 2,
    Applied = 3,
    RefundPending = 4,
    Refunded = 5
}
'@

# ============================================================
# DOMAIN ENTITY
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Entities\DiscountCode.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class DiscountCode : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }

    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public bool IsActive { get; set; } = true;
}
'@

# ============================================================
# INFRASTRUCTURE CONFIGURATION
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Configurations\DiscountCodeConfiguration.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.ToTable("DiscountCodes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.EditionId)
            .IsRequired();

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.DiscountType)
            .IsRequired();

        builder.Property(d => d.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.ActivationType)
            .IsRequired();

        builder.Property(d => d.ActivationThreshold);

        builder.Property(d => d.MaxUses);

        builder.Property(d => d.CurrentUses)
            .IsRequired();

        builder.Property(d => d.StartsAt);

        builder.Property(d => d.EndsAt);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasIndex(d => d.EditionId);

        builder.HasIndex(d => new { d.EditionId, d.Code })
            .IsUnique();

        builder.HasOne(d => d.Edition)
            .WithMany()
            .HasForeignKey(d => d.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
'@

# ============================================================
# APPLICATION REPOSITORY INTERFACE
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Contracts\Repositories\IDiscountCodeRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface IDiscountCodeRepository
{
    Task AddAsync(DiscountCode discountCode, CancellationToken cancellationToken = default);
    Task<DiscountCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiscountCode?> GetByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscountCode>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default);
    void Update(DiscountCode discountCode);
    void Delete(DiscountCode discountCode);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

# ============================================================
# INFRASTRUCTURE REPOSITORY
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Repositories\DiscountCodeRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly FestivalManagerDbContext _context;

    public DiscountCodeRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DiscountCode discountCode, CancellationToken cancellationToken = default)
    {
        await _context.DiscountCodes.AddAsync(discountCode, cancellationToken);
    }

    public async Task<DiscountCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DiscountCode?> GetByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.DiscountCodes.FirstOrDefaultAsync(d => d.EditionId == editionId && d.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.OrderBy(d => d.Code).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DiscountCode>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.Where(d => d.EditionId == editionId).OrderBy(d => d.Code).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.DiscountCodes.AnyAsync(d => d.EditionId == editionId && d.Code == normalizedCode, cancellationToken);
    }

    public void Update(DiscountCode discountCode)
    {
        _context.DiscountCodes.Update(discountCode);
    }

    public void Delete(DiscountCode discountCode)
    {
        _context.DiscountCodes.Remove(discountCode);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

# ============================================================
# FEATURE FILES
# ============================================================

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\DTOs\DiscountCodeDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.DTOs;

public class DiscountCodeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Requests\CreateDiscountCodeRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Requests;

public class CreateDiscountCodeRequest
{
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Requests\UpdateDiscountCodeRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Requests;

public class UpdateDiscountCodeRequest
{
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\CreateDiscountCode\CreateDiscountCodeCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;

public class CreateDiscountCodeCommand
{
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\UpdateDiscountCode\UpdateDiscountCodeCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.UpdateDiscountCode;

public class UpdateDiscountCodeCommand
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\DeleteDiscountCode\DeleteDiscountCodeCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.DeleteDiscountCode;

public class DeleteDiscountCodeCommand
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Queries\GetDiscountCodeById\GetDiscountCodeByIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodeById;

public class GetDiscountCodeByIdQuery
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Queries\GetDiscountCodesByEditionId\GetDiscountCodesByEditionIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodesByEditionId;

public class GetDiscountCodesByEditionIdQuery
{
    public Guid EditionId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\CreateDiscountCodeResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class CreateDiscountCodeResponse
{
    public DiscountCodeDto DiscountCode { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\UpdateDiscountCodeResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class UpdateDiscountCodeResponse
{
    public DiscountCodeDto DiscountCode { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\DeleteDiscountCodeResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class DeleteDiscountCodeResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\GetDiscountCodeByIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class GetDiscountCodeByIdResponse
{
    public DiscountCodeDto DiscountCode { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\GetDiscountCodesResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class GetDiscountCodesResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Contracts\Responses\GetDiscountCodesByEditionIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.Responses;

public class GetDiscountCodesByEditionIdResponse
{
    public IReadOnlyList<DiscountCodeDto> DiscountCodes { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Mappings\DiscountCodeMappingProfile.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Mappings;

public class DiscountCodeMappingProfile : Profile
{
    public DiscountCodeMappingProfile()
    {
        CreateMap<DiscountCode, DiscountCodeDto>();

        CreateMap<CreateDiscountCodeRequest, CreateDiscountCodeCommand>();
        CreateMap<UpdateDiscountCodeRequest, UpdateDiscountCodeCommand>();

        CreateMap<CreateDiscountCodeCommand, DiscountCode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentUses, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateDiscountCodeCommand, DiscountCode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore());
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Validators\CreateDiscountCodeCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Validators;

public class CreateDiscountCodeCommandValidator : AbstractValidator<CreateDiscountCodeCommand>
{
    public CreateDiscountCodeCommandValidator()
    {
        RuleFor(d => d.EditionId).NotEmpty();
        RuleFor(d => d.Code).NotEmpty().MaximumLength(50);
        RuleFor(d => d.Name).NotEmpty().MaximumLength(150);
        RuleFor(d => d.Description).MaximumLength(1000);
        RuleFor(d => d.DiscountType).IsInEnum();
        RuleFor(d => d.DiscountValue).GreaterThan(0);
        RuleFor(d => d.ActivationType).IsInEnum();
        RuleFor(d => d.ActivationThreshold).GreaterThan(0).When(d => d.ActivationType == DiscountActivationType.AfterThreshold);
        RuleFor(d => d.ActivationThreshold).Null().When(d => d.ActivationType == DiscountActivationType.Immediate);
        RuleFor(d => d.MaxUses).GreaterThan(0).When(d => d.MaxUses.HasValue);
        RuleFor(d => d.EndsAt).GreaterThan(d => d.StartsAt).When(d => d.StartsAt.HasValue && d.EndsAt.HasValue);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Validators\UpdateDiscountCodeCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Validators;

public class UpdateDiscountCodeCommandValidator : AbstractValidator<UpdateDiscountCodeCommand>
{
    public UpdateDiscountCodeCommandValidator()
    {
        RuleFor(d => d.Id).NotEmpty();
        RuleFor(d => d.EditionId).NotEmpty();
        RuleFor(d => d.Code).NotEmpty().MaximumLength(50);
        RuleFor(d => d.Name).NotEmpty().MaximumLength(150);
        RuleFor(d => d.Description).MaximumLength(1000);
        RuleFor(d => d.DiscountType).IsInEnum();
        RuleFor(d => d.DiscountValue).GreaterThan(0);
        RuleFor(d => d.ActivationType).IsInEnum();
        RuleFor(d => d.ActivationThreshold).GreaterThan(0).When(d => d.ActivationType == DiscountActivationType.AfterThreshold);
        RuleFor(d => d.ActivationThreshold).Null().When(d => d.ActivationType == DiscountActivationType.Immediate);
        RuleFor(d => d.MaxUses).GreaterThan(0).When(d => d.MaxUses.HasValue);
        RuleFor(d => d.CurrentUses).GreaterThanOrEqualTo(0);
        RuleFor(d => d.EndsAt).GreaterThan(d => d.StartsAt).When(d => d.StartsAt.HasValue && d.EndsAt.HasValue);
    }
}
'@

# Handlers/services/controller are in a secondary block to keep the script concise enough for editing.
# They are generated below.

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\CreateDiscountCode\CreateDiscountCodeHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;

public class CreateDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string normalizedCode = command.Code.Trim().ToUpperInvariant();

        bool exists = await _discountCodeRepository.ExistsByEditionAndCodeAsync(command.EditionId, normalizedCode, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Discount code '{normalizedCode}' already exists for this edition.");
        }

        command.Code = normalizedCode;

        DiscountCode discountCode = _mapper.Map<DiscountCode>(command);
        discountCode.CurrentUses = 0;
        discountCode.IsActive = true;

        await _discountCodeRepository.AddAsync(discountCode, cancellationToken);
        await _discountCodeRepository.SaveChangesAsync(cancellationToken);

        DiscountCodeDto dto = _mapper.Map<DiscountCodeDto>(discountCode);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\UpdateDiscountCode\UpdateDiscountCodeHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.UpdateDiscountCode;

public class UpdateDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string normalizedCode = command.Code.Trim().ToUpperInvariant();

        DiscountCode? existing = await _discountCodeRepository.GetByEditionAndCodeAsync(command.EditionId, normalizedCode, cancellationToken);

        if (existing is not null && existing.Id != command.Id)
        {
            throw new BusinessRuleException($"Discount code '{normalizedCode}' already exists for this edition.");
        }

        command.Code = normalizedCode;

        _mapper.Map(command, discountCode);
        discountCode.SetUpdated();

        await _discountCodeRepository.SaveChangesAsync(cancellationToken);

        DiscountCodeDto dto = _mapper.Map<DiscountCodeDto>(discountCode);

        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Commands\DeleteDiscountCode\DeleteDiscountCodeHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.DeleteDiscountCode;

public class DeleteDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public DeleteDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository)
    {
        _discountCodeRepository = discountCodeRepository;
    }

    public async Task HandleAsync(DeleteDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{command.Id}' was not found.");
        }

        _discountCodeRepository.Delete(discountCode);
        await _discountCodeRepository.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Queries\GetDiscountCodeById\GetDiscountCodeByIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodeById;

public class GetDiscountCodeByIdHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodeByIdHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(GetDiscountCodeByIdQuery query, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(query.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{query.Id}' was not found.");
        }

        return _mapper.Map<DiscountCodeDto>(discountCode);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Queries\GetDiscountCodes\GetDiscountCodesHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodes;

public class GetDiscountCodesHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodesHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCode> discountCodes = await _discountCodeRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<DiscountCodeDto>>(discountCodes);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Queries\GetDiscountCodesByEditionId\GetDiscountCodesByEditionIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodesByEditionId;

public class GetDiscountCodesByEditionIdHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodesByEditionIdHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> HandleAsync(GetDiscountCodesByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCode> discountCodes = await _discountCodeRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DiscountCodeDto>>(discountCodes);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Services\IDiscountCodeService.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public interface IDiscountCodeService
{
    Task<ApiResponse<CreateDiscountCodeResponse>> CreateAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateDiscountCodeResponse>> UpdateAsync(Guid id, UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteDiscountCodeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\DiscountCodes\Services\DiscountCodeService.cs" @'
namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly CreateDiscountCodeHandler _createDiscountCodeHandler;
    private readonly GetDiscountCodeByIdHandler _getDiscountCodeByIdHandler;
    private readonly GetDiscountCodesHandler _getDiscountCodesHandler;
    private readonly GetDiscountCodesByEditionIdHandler _getDiscountCodesByEditionIdHandler;
    private readonly UpdateDiscountCodeHandler _updateDiscountCodeHandler;
    private readonly DeleteDiscountCodeHandler _deleteDiscountCodeHandler;
    private readonly IValidator<CreateDiscountCodeCommand> _createDiscountCodeValidator;
    private readonly IValidator<UpdateDiscountCodeCommand> _updateDiscountCodeValidator;

    public DiscountCodeService(CreateDiscountCodeHandler createDiscountCodeHandler, GetDiscountCodeByIdHandler getDiscountCodeByIdHandler, GetDiscountCodesHandler getDiscountCodesHandler, GetDiscountCodesByEditionIdHandler getDiscountCodesByEditionIdHandler, UpdateDiscountCodeHandler updateDiscountCodeHandler, DeleteDiscountCodeHandler deleteDiscountCodeHandler, IValidator<CreateDiscountCodeCommand> createDiscountCodeValidator, IValidator<UpdateDiscountCodeCommand> updateDiscountCodeValidator)
    {
        _createDiscountCodeHandler = createDiscountCodeHandler;
        _getDiscountCodeByIdHandler = getDiscountCodeByIdHandler;
        _getDiscountCodesHandler = getDiscountCodesHandler;
        _getDiscountCodesByEditionIdHandler = getDiscountCodesByEditionIdHandler;
        _updateDiscountCodeHandler = updateDiscountCodeHandler;
        _deleteDiscountCodeHandler = deleteDiscountCodeHandler;
        _createDiscountCodeValidator = createDiscountCodeValidator;
        _updateDiscountCodeValidator = updateDiscountCodeValidator;
    }

    public async Task<ApiResponse<CreateDiscountCodeResponse>> CreateAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createDiscountCodeValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<CreateDiscountCodeResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        DiscountCodeDto discountCodeDto = await _createDiscountCodeHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new CreateDiscountCodeResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} is correctly registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DiscountCodeDto discountCodeDto = await _getDiscountCodeByIdHandler.HandleAsync(new GetDiscountCodeByIdQuery { Id = id }, cancellationToken);

        ApiResponse<GetDiscountCodeByIdResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodeByIdResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} retrieved successfully"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCodeDto> discountCodes = await _getDiscountCodesHandler.HandleAsync(cancellationToken);

        ApiResponse<GetDiscountCodesResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodesResponse { DiscountCodes = discountCodes },
            Errors = [],
            Message = $"There are {discountCodes.Count} discount codes registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCodeDto> discountCodes = await _getDiscountCodesByEditionIdHandler.HandleAsync(new GetDiscountCodesByEditionIdQuery { EditionId = editionId }, cancellationToken);

        ApiResponse<GetDiscountCodesByEditionIdResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodesByEditionIdResponse { DiscountCodes = discountCodes },
            Errors = [],
            Message = $"There are {discountCodes.Count} discount codes for this edition"
        };

        return response;
    }

    public async Task<ApiResponse<UpdateDiscountCodeResponse>> UpdateAsync(Guid id, UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        ValidationResult validationResult = await _updateDiscountCodeValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ApiResponse<UpdateDiscountCodeResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed");
        }

        DiscountCodeDto discountCodeDto = await _updateDiscountCodeHandler.HandleAsync(command, cancellationToken);

        ApiResponse<UpdateDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new UpdateDiscountCodeResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} updated successfully"
        };

        return response;
    }

    public async Task<ApiResponse<DeleteDiscountCodeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteDiscountCodeHandler.HandleAsync(new DeleteDiscountCodeCommand { Id = id }, cancellationToken);

        ApiResponse<DeleteDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new DeleteDiscountCodeResponse { Id = id, Deleted = true },
            Errors = [],
            Message = "Discount code deleted successfully"
        };

        return response;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Api\Controllers\DiscountCodesController.cs" @'
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/discount-codes")]
public class DiscountCodesController : ControllerBase
{
    private readonly IDiscountCodeService _discountCodeService;
    private readonly IMapper _mapper;

    public DiscountCodesController(IDiscountCodeService discountCodeService, IMapper mapper)
    {
        _discountCodeService = discountCodeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetDiscountCodesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodesResponse> response = await _discountCodeService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetDiscountCodeByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodeByIdResponse> response = await _discountCodeService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetDiscountCodesByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodesByEditionIdResponse> response = await _discountCodeService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateDiscountCodeResponse>>> Create([FromBody] CreateDiscountCodeRequest request, CancellationToken cancellationToken)
    {
        CreateDiscountCodeCommand command = _mapper.Map<CreateDiscountCodeCommand>(request);
        ApiResponse<CreateDiscountCodeResponse> response = await _discountCodeService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateDiscountCodeResponse>>> Update(Guid id, [FromBody] UpdateDiscountCodeRequest request, CancellationToken cancellationToken)
    {
        UpdateDiscountCodeCommand command = _mapper.Map<UpdateDiscountCodeCommand>(request);
        ApiResponse<UpdateDiscountCodeResponse> response = await _discountCodeService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteDiscountCodeResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteDiscountCodeResponse> response = await _discountCodeService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
'@

$dbContextPath = "$root\Alakai.FestivalManager.Infrastructure\Persistence\FestivalManagerDbContext.cs"
Insert-AfterIfMissing $dbContextPath "public DbSet<DiscountCode> DiscountCodes" "    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();" "    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();"

New-FileIfNotExists "$root\NEXT_STEPS_DISCOUNT_CODES.txt" @'
DiscountCodes backend generated.

Manual steps still required:

1. Add DI in ApplicationDependencyInjectionExtension:
   services.AddScoped<CreateDiscountCodeHandler>();
   services.AddScoped<GetDiscountCodeByIdHandler>();
   services.AddScoped<GetDiscountCodesHandler>();
   services.AddScoped<GetDiscountCodesByEditionIdHandler>();
   services.AddScoped<UpdateDiscountCodeHandler>();
   services.AddScoped<DeleteDiscountCodeHandler>();
   services.AddScoped<IDiscountCodeService, DiscountCodeService>();

2. Add DI in InfrastructureDependencyInjectionExtension:
   services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();

3. Add these properties manually to Registration entity:
   public Guid? DiscountCodeId { get; set; }
   public DiscountCode? DiscountCode { get; set; }
   public string? DiscountCodeValue { get; set; }
   public DiscountApplicationStatus DiscountStatus { get; set; }

4. Add these properties manually to RegistrationConfiguration:
   builder.Property(r => r.DiscountCodeValue).HasMaxLength(100);
   builder.Property(r => r.DiscountStatus).IsRequired();
   builder.HasIndex(r => r.DiscountCodeId);
   builder.HasOne(r => r.DiscountCode).WithMany().HasForeignKey(r => r.DiscountCodeId).OnDelete(DeleteBehavior.Restrict);

5. Add to Registration DTO/Request/Command as needed:
   Guid? DiscountCodeId
   string? DiscountCodeValue
   DiscountApplicationStatus DiscountStatus

6. Migration:
   dotnet ef migrations add AddDiscountCodes -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api

7. Update database:
   dotnet ef database update -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api

Next phase:
- Add discount calculation service.
- Integrate it into CreateRegistrationHandler.
'@

Write-Host ""
Write-Host "DiscountCodes backend generated."
Write-Host "Only DI, Registration manual fields, and migrations remain."
