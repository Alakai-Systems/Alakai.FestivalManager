namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public interface IEmailTemplateRendererService
{
    string Render(string template, Dictionary<string, string> variables);
}
