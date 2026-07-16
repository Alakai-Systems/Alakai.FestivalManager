namespace Alakai.FestivalManager.Application.Features.Festivals.Contracts.DTOs;

/// <summary>
/// Nunca incluye RedsysSecretKey ni EmailPassword en texto plano - solo indica
/// si ya estan configurados (HasRedsysSecretKey / HasEmailPassword).
/// </summary>
public class FestivalCredentialsDto
{
    public Guid FestivalId { get; set; }
    public string RedsysMerchantCode { get; set; } = string.Empty;
    public string RedsysTerminal { get; set; } = string.Empty;
    public bool HasRedsysSecretKey { get; set; }
    public string RedsysMerchantName { get; set; } = string.Empty;
    public string EmailHost { get; set; } = string.Empty;
    public int EmailPort { get; set; }
    public string EmailUsername { get; set; } = string.Empty;
    public bool HasEmailPassword { get; set; }
    public string EmailFromEmail { get; set; } = string.Empty;
    public string EmailFromName { get; set; } = string.Empty;
    public bool EmailUseSSL { get; set; }
}