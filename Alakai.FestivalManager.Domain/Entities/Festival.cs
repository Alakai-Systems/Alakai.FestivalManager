namespace Alakai.FestivalManager.Domain.Entities;

public class Festival : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>GA4 Property ID for this festival (e.g. "368043001"). Overrides the global PropertyId in appsettings when set.</summary>
    public string? GoogleAnalyticsPropertyId { get; set; }
    public string? TermsUrl { get; set; }
    public string? FaviconUrl { get; set; }

    /// <summary>Dominio propio del festival (ej. "app.lajambarcelona.com"), sin esquema ni ruta. Vacio = usa el dominio de Azure por defecto.</summary>
    public string? CustomDomain { get; set; }

    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions;
    public ICollection<Edition> Editions { get; set; } = new List<Edition>();
    public FestivalCredentials? Credentials { get; set; }
}