namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Queries.GetEmailTemplateById;

public class GetEmailTemplateByIdHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IMapper _mapper;

    public GetEmailTemplateByIdHandler(IEmailTemplateRepository emailTemplateRepository, IMapper mapper)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto?> HandleAsync(GetEmailTemplateByIdQuery query, CancellationToken cancellationToken = default)
    {
        EmailTemplate? emailTemplate = await _emailTemplateRepository.GetByIdAsync(query.Id, cancellationToken);

        return emailTemplate is null ? null : _mapper.Map<EmailTemplateDto>(emailTemplate);
    }
}
