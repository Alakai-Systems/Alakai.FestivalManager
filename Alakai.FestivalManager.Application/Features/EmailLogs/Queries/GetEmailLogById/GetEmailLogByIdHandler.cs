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
