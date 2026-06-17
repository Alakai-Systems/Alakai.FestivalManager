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
