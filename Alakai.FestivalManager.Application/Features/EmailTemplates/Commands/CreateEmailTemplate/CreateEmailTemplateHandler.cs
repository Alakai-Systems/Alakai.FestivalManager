namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Commands.CreateEmailTemplate;

public class CreateEmailTemplateHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateEmailTemplateHandler(IEmailTemplateRepository emailTemplateRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto> HandleAsync(CreateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        if (command.EditionId.HasValue)
        {
            Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId.Value, cancellationToken);

            if (edition is null)
            {
                throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
            }
        }

        bool exists = await _emailTemplateRepository.ExistsByEditionAndTemplateKeyAsync(command.EditionId, command.TemplateKey, null, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"An email template with key '{command.TemplateKey}' already exists for this edition scope.");
        }

        EmailTemplate emailTemplate = _mapper.Map<EmailTemplate>(command);
        emailTemplate.IsActive = true;

        await _emailTemplateRepository.AddAsync(emailTemplate, cancellationToken);
        await _emailTemplateRepository.SaveChangesAsync(cancellationToken);

        EmailTemplateDto emailTemplateDto = _mapper.Map<EmailTemplateDto>(emailTemplate);

        return emailTemplateDto;
    }
}
