namespace Alakai.FestivalManager.Domain.Entities;

public class EmailLayout : BaseEntity
{
    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }

    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }
}