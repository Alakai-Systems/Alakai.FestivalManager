namespace Alakai.FestivalManager.Admin.Contracts.Festivals.Requests;

public class UpdateFestivalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? TermsUrl { get; set; }
    public bool IsActive { get; set; }
    public int EnabledModules { get; set; }
}