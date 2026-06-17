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
