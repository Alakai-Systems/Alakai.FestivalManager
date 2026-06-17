namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Queries.GetEmailTemplatesByEditionId;

public class GetEmailTemplatesByEditionIdHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;

    public GetEmailTemplatesByEditionIdHandler(IEmailTemplateRepository emailTemplateRepository, IMapper mapper)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> HandleAsync(GetEmailTemplatesByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailTemplate> emailTemplates = await _emailTemplateRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        return _mapper.Map<IReadOnlyList<EmailTemplateDto>>(emailTemplates);
    }
}
