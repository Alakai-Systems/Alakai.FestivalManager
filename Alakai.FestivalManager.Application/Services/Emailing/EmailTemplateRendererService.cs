namespace Alakai.FestivalManager.Application.Services.Emailing;

public class EmailTemplateRendererService : IEmailTemplateRendererService
{
    public string Render(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        StringBuilder result = new(template);

        foreach (KeyValuePair<string, string> variable in variables)
        {
            result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
        }

        return result.ToString();
    }
}
