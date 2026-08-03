namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public class HmacTicketTokenService : ITicketTokenService
{
    private readonly TicketSecurityOptions _options;

    public HmacTicketTokenService(IOptions<TicketSecurityOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(Guid registrationId)
    {
        string payload = registrationId.ToString("D");
        byte[] signature = Sign(payload);

        return $"{payload}.{ToUrlSafeBase64(signature)}";
    }

    public bool TryValidateToken(string token, out Guid registrationId)
    {
        registrationId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string[] parts = token.Split('.', 2);

        if (parts.Length != 2 || !Guid.TryParseExact(parts[0], "D", out Guid parsedId))
        {
            return false;
        }

        byte[]? providedSignature = FromUrlSafeBase64(parts[1]);

        if (providedSignature is null)
        {
            return false;
        }

        byte[] expectedSignature = Sign(parts[0]);

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
        {
            return false;
        }

        registrationId = parsedId;

        return true;
    }

    private byte[] Sign(string payload)
    {
        byte[] key = Encoding.UTF8.GetBytes(_options.SecretKey);
        using HMACSHA256 hmac = new(key);

        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    }

    private static string ToUrlSafeBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[]? FromUrlSafeBase64(string value)
    {
        try
        {
            string normalized = value.Replace('-', '+').Replace('_', '/');
            int padding = normalized.Length % 4;

            if (padding == 2) normalized += "==";
            else if (padding == 3) normalized += "=";
            else if (padding != 0) return null;

            return Convert.FromBase64String(normalized);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}