namespace Alakai.FestivalManager.Domain.Entities;

/// <summary>
/// Credenciales sensibles de integracion (Redsys + Email) especificas de cada Festival.
/// Deliberadamente separada de Festival para que nunca pueda filtrarse por accidente
/// en un DTO publico (registro, listado de festivales, etc).
/// </summary>
public class FestivalCredentials : BaseEntity
{
    public Guid FestivalId { get; set; }
    public Festival Festival { get; set; } = null!;

    // Redsys
    public string RedsysMerchantCode { get; set; } = string.Empty;
    public string RedsysTerminal { get; set; } = string.Empty;
    public string RedsysSecretKey { get; set; } = string.Empty;
    public string RedsysMerchantName { get; set; } = string.Empty;

    /// <summary>URL del endpoint de Redsys para este festival (test o produccion). Vacio = usa el fallback global.</summary>
    public string? RedsysPaymentUrl { get; set; }

    // Email (SMTP)
    public string EmailHost { get; set; } = string.Empty;
    public int EmailPort { get; set; } = 587;
    public string EmailUsername { get; set; } = string.Empty;
    public string EmailPassword { get; set; } = string.Empty;
    public string EmailFromEmail { get; set; } = string.Empty;
    public string EmailFromName { get; set; } = string.Empty;
    public bool EmailUseSSL { get; set; } = true;
}