namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

public class DeleteEmailTemplateCommand
{
    public Guid Id { get; set; }

    public DeleteEmailTemplateCommand(Guid id)
    {
        Id = id;
    }
}
