namespace Alakai.FestivalManager.Application.Features.Emails.Models;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    public string Render(string template, EmailTemplateModel model)
    {
        string html = template;

        foreach (KeyValuePair<string, string> item in model.Values)
        {
            html = html.Replace(item.Key, item.Value);
        }

        return html;
    }
}