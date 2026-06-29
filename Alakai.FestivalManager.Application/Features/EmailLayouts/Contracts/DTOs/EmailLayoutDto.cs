namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.DTOs;

public class EmailLayoutDto
{
    public Guid Id { get; set; }
    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}