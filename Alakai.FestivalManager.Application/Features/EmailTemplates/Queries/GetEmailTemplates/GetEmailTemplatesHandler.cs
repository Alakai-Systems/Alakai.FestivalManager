namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Queries.GetEmailTemplates;

public class GetEmailTemplatesHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;

    public GetEmailTemplatesHandler(IEmailTemplateRepository emailTemplateRepository, IMapper mapper)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> HandleAsync(GetEmailTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailTemplate> emailTemplates = await _emailTemplateRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<EmailTemplateDto>>(emailTemplates);
    }
}
