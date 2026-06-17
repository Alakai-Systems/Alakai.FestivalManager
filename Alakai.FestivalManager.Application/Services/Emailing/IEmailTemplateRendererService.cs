namespace Alakai.FestivalManager.Application.Services.Emailing;

public interface IEmailTemplateRendererService
{
    string Render(string template, Dictionary<string, string> variables);
}
