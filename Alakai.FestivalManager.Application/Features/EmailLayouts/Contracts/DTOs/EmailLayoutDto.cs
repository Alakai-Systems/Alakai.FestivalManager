namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.DTOs;

public class EmailLayoutDto
{
    public Guid Id { get; set; }
    public Guid? EditionId { get; set; }
    public string? EditionName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HeaderHtml { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string FooterHtml { get; set; } = string.Empty;
    public string? FooterText { get; set; }
    public int? HeaderImageWidth { get; set; }
    public int? FooterImageWidth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}