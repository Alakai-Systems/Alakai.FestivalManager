using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Festivals.Contracts.Requests;

public class CreateFestivalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? TermsUrl { get; set; }
    public string? GoogleAnalyticsPropertyId { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CustomDomain { get; set; }
    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions;
}