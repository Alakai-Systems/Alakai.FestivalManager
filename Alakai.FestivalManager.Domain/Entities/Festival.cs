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

    // Que planes de pago puede elegir quien se registra (ver EnabledPaymentPlan).
    // Los 3 activos por defecto, para no cambiar el comportamiento de los
    // festivales que ya existen -- hoy los 3 siempre estaban visibles y fijos.
    public EnabledPaymentPlan EnabledPaymentPlans { get; set; } =
        EnabledPaymentPlan.FullOnline | EnabledPaymentPlan.SplitFiftyFifty | EnabledPaymentPlan.DeferredTenDays;

    // Que plataformas de pago puede elegir quien se registra (ver EnabledPaymentPlatform).
    // Redsys activo por defecto, para no cambiar el comportamiento de los festivales
    // que ya existen -- hoy Redsys era la unica plataforma disponible.
    public EnabledPaymentPlatform EnabledPaymentPlatforms { get; set; } = EnabledPaymentPlatform.Redsys;

    public ICollection<Edition> Editions { get; set; } = new List<Edition>();
    public FestivalCredentials? Credentials { get; set; }
}