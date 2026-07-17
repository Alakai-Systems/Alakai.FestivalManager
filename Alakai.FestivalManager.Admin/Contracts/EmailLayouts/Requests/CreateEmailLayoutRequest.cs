namespace Alakai.FestivalManager.Admin.Contracts.EmailLayouts.Requests;

public class CreateEmailLayoutRequest
{
    public Guid? EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }
    public int? HeaderImageWidth { get; set; }
    public int? FooterImageWidth { get; set; }
    public bool IsActive { get; set; } = true;
}