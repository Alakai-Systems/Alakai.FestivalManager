namespace Alakai.FestivalManager.Admin.Contracts.Festivals.DTOs;

public class FestivalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? TermsUrl { get; set; }
    public bool IsActive { get; set; }
    public int EnabledModules { get; set; }
}