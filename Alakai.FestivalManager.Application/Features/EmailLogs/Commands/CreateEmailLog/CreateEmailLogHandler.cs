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
