namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

public class DeleteEmailTemplateHandler
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    public DeleteEmailTemplateHandler(IEmailTemplateRepository emailTemplateRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
    }

    public async Task<Guid> HandleAsync(DeleteEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        EmailTemplate? emailTemplate = await _emailTemplateRepository.GetByIdAsync(command.Id, cancellationToken);

        if (emailTemplate is null)
        {
            throw new NotFoundException($"Email template with id '{command.Id}' was not found.");
        }

        _emailTemplateRepository.Delete(emailTemplate);
        await _emailTemplateRepository.SaveChangesAsync(cancellationToken);

        return emailTemplate.Id;
    }
}
