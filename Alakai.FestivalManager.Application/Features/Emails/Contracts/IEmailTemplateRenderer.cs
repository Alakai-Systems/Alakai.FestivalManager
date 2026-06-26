namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public interface IEmailTemplateRenderer
{
    string Render(string template, EmailTemplateModel model);
}
