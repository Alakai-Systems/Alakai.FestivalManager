namespace Alakai.FestivalManager.Application.Features.Festivals.Contracts.Requests;

public class UpsertFestivalCredentialsRequest
{
    public string RedsysMerchantCode { get; set; } = string.Empty;
    public string RedsysTerminal { get; set; } = string.Empty;
    public string? RedsysSecretKey { get; set; }
    public string RedsysMerchantName { get; set; } = string.Empty;
    public string EmailHost { get; set; } = string.Empty;
    public int EmailPort { get; set; }
    public string EmailUsername { get; set; } = string.Empty;
    public string? EmailPassword { get; set; }
    public string EmailFromEmail { get; set; } = string.Empty;
    public string EmailFromName { get; set; } = string.Empty;
    public bool EmailUseSSL { get; set; } = true;
}