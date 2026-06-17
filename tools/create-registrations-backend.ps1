param(
    [string]$SolutionRoot = "."
)

$ErrorActionPreference = "Stop"

function New-FileIfNotExists {
    param([string]$Path, [string]$Content)
    $directory = Split-Path $Path -Parent
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    if (-not (Test-Path $Path)) { Set-Content -Path $Path -Value $Content -Encoding UTF8; Write-Host "Created: $Path" }
    else { Write-Host "Skipped existing: $Path" }
}

function Insert-AfterIfMissing {
    param([string]$Path, [string]$SearchText, [string]$InsertText, [string]$AnchorText)
    if (-not (Test-Path $Path)) { Write-Host "Skipped insert. File not found: $Path"; return }
    $content = Get-Content -Path $Path -Raw
    if ($content.Contains($SearchText)) { Write-Host "Already present: $SearchText"; return }
    if ($content.Contains($AnchorText)) { $content = $content.Replace($AnchorText, "$AnchorText`r`n$InsertText"); Set-Content -Path $Path -Value $content -Encoding UTF8; Write-Host "Patched: $Path" }
    else { Write-Host "Skipped insert. Anchor not found in: $Path" }
}

$root = Resolve-Path $SolutionRoot

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Enums\EmailLogStatus.cs" @'
namespace Alakai.FestivalManager.Domain.Enums;

public enum EmailLogStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Skipped = 4
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Domain\Entities\EmailLog.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class EmailLog : BaseEntity
{
    public Guid? EditionId { get; set; }
    public Edition? Edition { get; set; }

    public Guid? EmailTemplateId { get; set; }
    public EmailTemplate? EmailTemplate { get; set; }

    public Guid? RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; } = true;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Configurations\EmailLogConfiguration.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TemplateKey).IsRequired();
        builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(200);
        builder.Property(e => e.RecipientName).HasMaxLength(200);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.BodyHtml).IsRequired();
        builder.Property(e => e.BodyText);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.SentAt);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.EditionId);
        builder.HasIndex(e => e.EmailTemplateId);
        builder.HasIndex(e => e.RegistrationId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TemplateKey);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RecipientEmail);
        builder.HasOne(e => e.Edition).WithMany().HasForeignKey(e => e.EditionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmailTemplate).WithMany().HasForeignKey(e => e.EmailTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Registration).WithMany().HasForeignKey(e => e.RegistrationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Contracts\Repositories\IEmailLogRepository.cs" @'
namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface IEmailLogRepository
{
    Task AddAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Update(EmailLog emailLog);
    void Delete(EmailLog emailLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Infrastructure\Repositories\EmailLogRepository.cs" @'
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly FestivalManagerDbContext _context;

    public EmailLogRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        await _context.EmailLogs.AddAsync(emailLog, cancellationToken);
    }

    public async Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.EditionId == editionId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.RegistrationId == registrationId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public void Update(EmailLog emailLog)
    {
        _context.EmailLogs.Update(emailLog);
    }

    public void Delete(EmailLog emailLog)
    {
        _context.EmailLogs.Remove(emailLog);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

# Contracts
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\DTOs\EmailLogDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.DTOs;

public class EmailLogDto
{
    public Guid Id { get; set; }
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Requests\CreateEmailLogRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Requests;

public class CreateEmailLogRequest
{
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Requests\UpdateEmailLogRequest.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Requests;

public class UpdateEmailLogRequest
{
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\CreateEmailLogResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class CreateEmailLogResponse
{
    public EmailLogDto EmailLog { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\UpdateEmailLogResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class UpdateEmailLogResponse
{
    public EmailLogDto EmailLog { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\DeleteEmailLogResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class DeleteEmailLogResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\GetEmailLogByIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogByIdResponse
{
    public EmailLogDto EmailLog { get; set; } = default!;
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\GetEmailLogsResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\GetEmailLogsByEditionIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByEditionIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\GetEmailLogsByRegistrationIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByRegistrationIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Contracts\Responses\GetEmailLogsByUserIdResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByUserIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
'@

# Commands
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\CreateEmailLog\CreateEmailLogCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.CreateEmailLog;

public class CreateEmailLogCommand
{
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\UpdateEmailLog\UpdateEmailLogCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.UpdateEmailLog;

public class UpdateEmailLogCommand
{
    public Guid Id { get; set; }
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\DeleteEmailLog\DeleteEmailLogCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.DeleteEmailLog;

public class DeleteEmailLogCommand
{
    public Guid Id { get; set; }
}
'@

# Queries
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogById\GetEmailLogByIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogById;

public class GetEmailLogByIdQuery
{
    public Guid Id { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByEditionId\GetEmailLogsByEditionIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByEditionId;

public class GetEmailLogsByEditionIdQuery
{
    public Guid EditionId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByRegistrationId\GetEmailLogsByRegistrationIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByRegistrationId;

public class GetEmailLogsByRegistrationIdQuery
{
    public Guid RegistrationId { get; set; }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByUserId\GetEmailLogsByUserIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByUserId;

public class GetEmailLogsByUserIdQuery
{
    public Guid UserId { get; set; }
}
'@

# Mapping
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Mappings\EmailLogMappingProfile.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Mappings;

public class EmailLogMappingProfile : Profile
{
    public EmailLogMappingProfile()
    {
        CreateMap<EmailLog, EmailLogDto>();
        CreateMap<CreateEmailLogRequest, CreateEmailLogCommand>();
        CreateMap<UpdateEmailLogRequest, UpdateEmailLogCommand>();

        CreateMap<CreateEmailLogCommand, EmailLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateEmailLogCommand, EmailLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
'@

# Validators
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Validators\CreateEmailLogCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Validators;

public class CreateEmailLogCommandValidator : AbstractValidator<CreateEmailLogCommand>
{
    public CreateEmailLogCommandValidator()
    {
        RuleFor(e => e.TemplateKey).IsInEnum();
        RuleFor(e => e.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(e => e.RecipientName).MaximumLength(200);
        RuleFor(e => e.Subject).NotEmpty().MaximumLength(300);
        RuleFor(e => e.BodyHtml).NotEmpty();
        RuleFor(e => e.Status).IsInEnum();
        RuleFor(e => e.ErrorMessage).MaximumLength(2000);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Validators\UpdateEmailLogCommandValidator.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Validators;

public class UpdateEmailLogCommandValidator : AbstractValidator<UpdateEmailLogCommand>
{
    public UpdateEmailLogCommandValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.TemplateKey).IsInEnum();
        RuleFor(e => e.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(e => e.RecipientName).MaximumLength(200);
        RuleFor(e => e.Subject).NotEmpty().MaximumLength(300);
        RuleFor(e => e.BodyHtml).NotEmpty();
        RuleFor(e => e.Status).IsInEnum();
        RuleFor(e => e.ErrorMessage).MaximumLength(2000);
    }
}
'@

# Handlers
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\CreateEmailLog\CreateEmailLogHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.CreateEmailLog;

public class CreateEmailLogHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public CreateEmailLogHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto> HandleAsync(CreateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        EmailLog emailLog = _mapper.Map<EmailLog>(command);
        emailLog.IsActive = true;
        await _emailLogRepository.AddAsync(emailLog, cancellationToken);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);
        EmailLogDto dto = _mapper.Map<EmailLogDto>(emailLog);
        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\UpdateEmailLog\UpdateEmailLogHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.UpdateEmailLog;

public class UpdateEmailLogHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public UpdateEmailLogHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto> HandleAsync(UpdateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(command.Id, cancellationToken);
        if (emailLog is null) { throw new NotFoundException($"Email log with id '{command.Id}' was not found."); }
        _mapper.Map(command, emailLog);
        emailLog.SetUpdated();
        await _emailLogRepository.SaveChangesAsync(cancellationToken);
        EmailLogDto dto = _mapper.Map<EmailLogDto>(emailLog);
        return dto;
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Commands\DeleteEmailLog\DeleteEmailLogHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.DeleteEmailLog;

public class DeleteEmailLogHandler
{
    private readonly IEmailLogRepository _emailLogRepository;

    public DeleteEmailLogHandler(IEmailLogRepository emailLogRepository)
    {
        _emailLogRepository = emailLogRepository;
    }

    public async Task HandleAsync(DeleteEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(command.Id, cancellationToken);
        if (emailLog is null) { throw new NotFoundException($"Email log with id '{command.Id}' was not found."); }
        emailLog.IsActive = false;
        emailLog.SetUpdated();
        await _emailLogRepository.SaveChangesAsync(cancellationToken);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogById\GetEmailLogByIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogById;

public class GetEmailLogByIdHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public GetEmailLogByIdHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto> HandleAsync(GetEmailLogByIdQuery query, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(query.Id, cancellationToken);
        if (emailLog is null) { throw new NotFoundException($"Email log with id '{query.Id}' was not found."); }
        return _mapper.Map<EmailLogDto>(emailLog);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogs\GetEmailLogsHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogs;

public class GetEmailLogsHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public GetEmailLogsHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailLogDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLog> emailLogs = await _emailLogRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EmailLogDto>>(emailLogs);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByEditionId\GetEmailLogsByEditionIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByEditionId;

public class GetEmailLogsByEditionIdHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public GetEmailLogsByEditionIdHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailLogDto>> HandleAsync(GetEmailLogsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLog> emailLogs = await _emailLogRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<EmailLogDto>>(emailLogs);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByRegistrationId\GetEmailLogsByRegistrationIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByRegistrationId;

public class GetEmailLogsByRegistrationIdHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public GetEmailLogsByRegistrationIdHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailLogDto>> HandleAsync(GetEmailLogsByRegistrationIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLog> emailLogs = await _emailLogRepository.GetByRegistrationIdAsync(query.RegistrationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<EmailLogDto>>(emailLogs);
    }
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Queries\GetEmailLogsByUserId\GetEmailLogsByUserIdHandler.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Queries.GetEmailLogsByUserId;

public class GetEmailLogsByUserIdHandler
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMapper _mapper;

    public GetEmailLogsByUserIdHandler(IEmailLogRepository emailLogRepository, IMapper mapper)
    {
        _emailLogRepository = emailLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailLogDto>> HandleAsync(GetEmailLogsByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLog> emailLogs = await _emailLogRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        return _mapper.Map<IReadOnlyList<EmailLogDto>>(emailLogs);
    }
}
'@

# Services
New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Services\IEmailLogService.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Services;

public interface IEmailLogService
{
    Task<ApiResponse<CreateEmailLogResponse>> CreateAsync(CreateEmailLogCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByUserIdResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEmailLogResponse>> UpdateAsync(Guid id, UpdateEmailLogCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteEmailLogResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
'@

New-FileIfNotExists "$root\Alakai.FestivalManager.Application\Features\EmailLogs\Services\EmailLogService.cs" @'
namespace Alakai.FestivalManager.Application.Features.EmailLogs.Services;

public class EmailLogService : IEmailLogService
{
    private readonly CreateEmailLogHandler _createEmailLogHandler;
    private readonly GetEmailLogByIdHandler _getEmailLogByIdHandler;
    private readonly GetEmailLogsHandler _getEmailLogsHandler;
    private readonly GetEmailLogsByEditionIdHandler _getEmailLogsByEditionIdHandler;
    private readonly GetEmailLogsByRegistrationIdHandler _getEmailLogsByRegistrationIdHandler;
    private readonly GetEmailLogsByUserIdHandler _getEmailLogsByUserIdHandler;
    private readonly UpdateEmailLogHandler _updateEmailLogHandler;
    private readonly DeleteEmailLogHandler _deleteEmailLogHandler;
    private readonly IValidator<CreateEmailLogCommand> _createEmailLogValidator;
    private readonly IValidator<UpdateEmailLogCommand> _updateEmailLogValidator;

    public EmailLogService(CreateEmailLogHandler createEmailLogHandler, GetEmailLogByIdHandler getEmailLogByIdHandler, GetEmailLogsHandler getEmailLogsHandler, GetEmailLogsByEditionIdHandler getEmailLogsByEditionIdHandler, GetEmailLogsByRegistrationIdHandler getEmailLogsByRegistrationIdHandler, GetEmailLogsByUserIdHandler getEmailLogsByUserIdHandler, UpdateEmailLogHandler updateEmailLogHandler, DeleteEmailLogHandler deleteEmailLogHandler, IValidator<CreateEmailLogCommand> createEmailLogValidator, IValidator<UpdateEmailLogCommand> updateEmailLogValidator)
    {
        _createEmailLogHandler = createEmailLogHandler;
        _getEmailLogByIdHandler = getEmailLogByIdHandler;
        _getEmailLogsHandler = getEmailLogsHandler;
        _getEmailLogsByEditionIdHandler = getEmailLogsByEditionIdHandler;
        _getEmailLogsByRegistrationIdHandler = getEmailLogsByRegistrationIdHandler;
        _getEmailLogsByUserIdHandler = getEmailLogsByUserIdHandler;
        _updateEmailLogHandler = updateEmailLogHandler;
        _deleteEmailLogHandler = deleteEmailLogHandler;
        _createEmailLogValidator = createEmailLogValidator;
        _updateEmailLogValidator = updateEmailLogValidator;
    }

    public async Task<ApiResponse<CreateEmailLogResponse>> CreateAsync(CreateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createEmailLogValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { return ApiResponse<CreateEmailLogResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed"); }
        EmailLogDto emailLogDto = await _createEmailLogHandler.HandleAsync(command, cancellationToken);
        ApiResponse<CreateEmailLogResponse> response = new() { Success = true, Data = new CreateEmailLogResponse { EmailLog = emailLogDto }, Errors = [], Message = "Email log created successfully" };
        return response;
    }

    public async Task<ApiResponse<GetEmailLogByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmailLogDto emailLogDto = await _getEmailLogByIdHandler.HandleAsync(new GetEmailLogByIdQuery { Id = id }, cancellationToken);
        ApiResponse<GetEmailLogByIdResponse> response = new() { Success = true, Data = new GetEmailLogByIdResponse { EmailLog = emailLogDto }, Errors = [], Message = "Email log retrieved successfully" };
        return response;
    }

    public async Task<ApiResponse<GetEmailLogsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsHandler.HandleAsync(cancellationToken);
        ApiResponse<GetEmailLogsResponse> response = new() { Success = true, Data = new GetEmailLogsResponse { EmailLogs = emailLogs }, Errors = [], Message = $"There are {emailLogs.Count} email logs registered" };
        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByEditionIdHandler.HandleAsync(new GetEmailLogsByEditionIdQuery { EditionId = editionId }, cancellationToken);
        ApiResponse<GetEmailLogsByEditionIdResponse> response = new() { Success = true, Data = new GetEmailLogsByEditionIdResponse { EmailLogs = emailLogs }, Errors = [], Message = $"There are {emailLogs.Count} email logs for this edition" };
        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByRegistrationIdHandler.HandleAsync(new GetEmailLogsByRegistrationIdQuery { RegistrationId = registrationId }, cancellationToken);
        ApiResponse<GetEmailLogsByRegistrationIdResponse> response = new() { Success = true, Data = new GetEmailLogsByRegistrationIdResponse { EmailLogs = emailLogs }, Errors = [], Message = $"There are {emailLogs.Count} email logs for this registration" };
        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByUserIdResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByUserIdHandler.HandleAsync(new GetEmailLogsByUserIdQuery { UserId = userId }, cancellationToken);
        ApiResponse<GetEmailLogsByUserIdResponse> response = new() { Success = true, Data = new GetEmailLogsByUserIdResponse { EmailLogs = emailLogs }, Errors = [], Message = $"There are {emailLogs.Count} email logs for this user" };
        return response;
    }

    public async Task<ApiResponse<UpdateEmailLogResponse>> UpdateAsync(Guid id, UpdateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        ValidationResult validationResult = await _updateEmailLogValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { return ApiResponse<UpdateEmailLogResponse>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList(), "Validation failed"); }
        EmailLogDto emailLogDto = await _updateEmailLogHandler.HandleAsync(command, cancellationToken);
        ApiResponse<UpdateEmailLogResponse> response = new() { Success = true, Data = new UpdateEmailLogResponse { EmailLog = emailLogDto }, Errors = [], Message = "Email log updated successfully" };
        return response;
    }

    public async Task<ApiResponse<DeleteEmailLogResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteEmailLogHandler.HandleAsync(new DeleteEmailLogCommand { Id = id }, cancellationToken);
        ApiResponse<DeleteEmailLogResponse> response = new() { Success = true, Data = new DeleteEmailLogResponse { Id = id, Deleted = true }, Errors = [], Message = "Email log deleted successfully" };
        return response;
    }
}
'@

# Controller
New-FileIfNotExists "$root\Alakai.FestivalManager.Api\Controllers\EmailLogsController.cs" @'
namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/email-logs")]
public class EmailLogsController : ControllerBase
{
    private readonly IEmailLogService _emailLogService;
    private readonly IMapper _mapper;

    public EmailLogsController(IEmailLogService emailLogService, IMapper mapper)
    {
        _emailLogService = emailLogService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetEmailLogsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsResponse> response = await _emailLogService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogByIdResponse> response = await _emailLogService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByEditionIdResponse> response = await _emailLogService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-registration/{registrationId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByRegistrationIdResponse>>> GetByRegistrationId(Guid registrationId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByRegistrationIdResponse> response = await _emailLogService.GetByRegistrationIdAsync(registrationId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<GetEmailLogsByUserIdResponse>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEmailLogsByUserIdResponse> response = await _emailLogService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateEmailLogResponse>>> Create([FromBody] CreateEmailLogRequest request, CancellationToken cancellationToken)
    {
        CreateEmailLogCommand command = _mapper.Map<CreateEmailLogCommand>(request);
        ApiResponse<CreateEmailLogResponse> response = await _emailLogService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateEmailLogResponse>>> Update(Guid id, [FromBody] UpdateEmailLogRequest request, CancellationToken cancellationToken)
    {
        UpdateEmailLogCommand command = _mapper.Map<UpdateEmailLogCommand>(request);
        ApiResponse<UpdateEmailLogResponse> response = await _emailLogService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteEmailLogResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEmailLogResponse> response = await _emailLogService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
'@

$dbContextPath = "$root\Alakai.FestivalManager.Infrastructure\Persistence\FestivalManagerDbContext.cs"
Insert-AfterIfMissing $dbContextPath "public DbSet<EmailLog> EmailLogs" "    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();" "    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();"

New-FileIfNotExists "$root\NEXT_STEPS_EMAIL_LOGS.txt" @'
EmailLogs backend generated.

Manual steps still required:

1. Register DI in ApplicationDependencyInjectionExtension:
   services.AddScoped<CreateEmailLogHandler>();
   services.AddScoped<GetEmailLogByIdHandler>();
   services.AddScoped<GetEmailLogsHandler>();
   services.AddScoped<GetEmailLogsByEditionIdHandler>();
   services.AddScoped<GetEmailLogsByRegistrationIdHandler>();
   services.AddScoped<GetEmailLogsByUserIdHandler>();
   services.AddScoped<UpdateEmailLogHandler>();
   services.AddScoped<DeleteEmailLogHandler>();
   services.AddScoped<IEmailLogService, EmailLogService>();

2. Register DI in InfrastructureDependencyInjectionExtension:
   services.AddScoped<IEmailLogRepository, EmailLogRepository>();

3. Add migration:
   dotnet ef migrations add AddEmailLogs -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api

4. Update database:
   dotnet ef database update -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api
'@

Write-Host ""
Write-Host "EmailLogs backend generated. Only DI and migrations remain manual."
