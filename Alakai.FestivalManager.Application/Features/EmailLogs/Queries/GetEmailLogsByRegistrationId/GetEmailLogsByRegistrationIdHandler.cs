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
