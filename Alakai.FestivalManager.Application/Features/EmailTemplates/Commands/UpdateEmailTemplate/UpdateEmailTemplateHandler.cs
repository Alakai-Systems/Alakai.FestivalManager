namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateEmailTemplateHandler(IEmailTemplateRepository emailTemplateRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto> HandleAsync(UpdateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        EmailTemplate? emailTemplate = await _emailTemplateRepository.GetByIdAsync(command.Id, cancellationToken);

        if (emailTemplate is null)
        {
            throw new NotFoundException($"Email template with id '{command.Id}' was not found.");
        }

        if (command.EditionId.HasValue)
        {
            Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId.Value, cancellationToken);

            if (edition is null)
            {
                throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
            }
        }

        bool exists = await _emailTemplateRepository.ExistsByEditionAndTemplateKeyAsync(command.EditionId, command.TemplateKey, command.Language, command.Id, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"An email template with key '{command.TemplateKey}' and language '{command.Language}' already exists for this edition scope.");
        }

        _mapper.Map(command, emailTemplate);
        emailTemplate.SetUpdated();

        await _emailTemplateRepository.SaveChangesAsync(cancellationToken);

        EmailTemplateDto emailTemplateDto = _mapper.Map<EmailTemplateDto>(emailTemplate);

        return emailTemplateDto;
    }
}
