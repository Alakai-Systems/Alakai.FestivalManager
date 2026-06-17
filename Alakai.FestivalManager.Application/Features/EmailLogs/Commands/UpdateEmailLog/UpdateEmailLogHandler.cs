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
