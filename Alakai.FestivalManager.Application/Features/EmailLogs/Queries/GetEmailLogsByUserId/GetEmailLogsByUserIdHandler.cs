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
