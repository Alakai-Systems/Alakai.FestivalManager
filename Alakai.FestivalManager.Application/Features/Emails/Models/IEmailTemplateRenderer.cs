namespace Alakai.FestivalManager.Application.Features.Emails.Models;

public interface IEmailTemplateRenderer
{
    string Render(string template, EmailTemplateModel model);
}
