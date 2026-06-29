namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Requests;

public class UpdateEmailLayoutRequest
{
    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }
}