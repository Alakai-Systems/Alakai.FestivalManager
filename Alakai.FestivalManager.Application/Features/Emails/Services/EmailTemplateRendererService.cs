namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public class EmailTemplateRendererService : IEmailTemplateRendererService
{
    public string Render(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        // Strip any HTML tags that a WYSIWYG editor may have injected inside {{placeholders}}.
        // e.g. {{Accommodation<b>BuildingName</b>}} -> {{AccommodationBuildingName}}
        string sanitized = System.Text.RegularExpressions.Regex.Replace(
            template,
            @"\{\{(.*?)\}\}",
            m =>
            {
                string inner = System.Text.RegularExpressions.Regex.Replace(m.Groups[1].Value, "<[^>]+>", string.Empty);
                inner = System.Net.WebUtility.HtmlDecode(inner).Trim();
                return "{{" + inner + "}}";
            },
            System.Text.RegularExpressions.RegexOptions.Singleline);

        StringBuilder result = new(sanitized);

        foreach (KeyValuePair<string, string> variable in variables)
        {
            result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
        }

        return result.ToString();
    }
}