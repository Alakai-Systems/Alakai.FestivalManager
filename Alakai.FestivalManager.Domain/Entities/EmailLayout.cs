namespace Alakai.FestivalManager.Domain.Entities;

public class EmailLayout : BaseEntity
{
    public Guid? EditionId { get; set; }
    public Edition? Edition { get; set; }

    public string Name { get; set; } = string.Empty;

    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }

    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }

    public bool IsActive { get; set; } = true;
}