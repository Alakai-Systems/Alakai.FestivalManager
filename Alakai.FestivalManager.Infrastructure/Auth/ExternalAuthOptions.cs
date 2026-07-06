namespace Alakai.FestivalManager.Infrastructure.Auth;

public class ExternalAuthOptions
{
    public ExternalAuthProviderOptions Google { get; set; } = new();
    public ExternalAuthProviderOptions Apple { get; set; } = new();
}

public class ExternalAuthProviderOptions
{
    public string ClientId { get; set; } = string.Empty;
}
