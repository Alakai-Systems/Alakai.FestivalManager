using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Infrastructure.Payments;

public class RedsysGateway : IRedsysGateway
{
    private const string SignatureVersion = "HMAC_SHA256_V1";

    private readonly RedsysOptions _options;
    private readonly ILogger<RedsysGateway> _logger;

    public RedsysGateway(IOptions<RedsysOptions> options, ILogger<RedsysGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public RedsysPaymentFormDto BuildPaymentForm(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string? urlOk = null, string? urlKo = null)
    {
        Dictionary<string, string> parameters = new()
        {
            ["DS_MERCHANT_AMOUNT"] = amountInCents.ToString(),
            ["DS_MERCHANT_ORDER"] = order,
            ["DS_MERCHANT_MERCHANTCODE"] = credentials.RedsysMerchantCode,
            ["DS_MERCHANT_CURRENCY"] = _options.Currency,
            ["DS_MERCHANT_TRANSACTIONTYPE"] = "0",
            ["DS_MERCHANT_TERMINAL"] = credentials.RedsysTerminal,
            ["DS_MERCHANT_MERCHANTURL"] = _options.NotificationUrl,
            ["DS_MERCHANT_URLOK"] = urlOk ?? _options.UrlOk,
            ["DS_MERCHANT_URLKO"] = urlKo ?? _options.UrlKo,
            ["DS_MERCHANT_PRODUCTDESCRIPTION"] = productDescription,
            ["DS_MERCHANT_MERCHANTNAME"] = credentials.RedsysMerchantName,
            ["DS_MERCHANT_CONSUMERLANGUAGE"] = "0"
        };

        string json = JsonSerializer.Serialize(parameters);
        string merchantParameters = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        string signature = Sign(order, merchantParameters, credentials.RedsysSecretKey);

        string paymentUrl = string.IsNullOrWhiteSpace(credentials.RedsysPaymentUrl)
            ? _options.PaymentUrl
            : credentials.RedsysPaymentUrl;

        return new RedsysPaymentFormDto
        {
            Url = paymentUrl,
            SignatureVersion = SignatureVersion,
            MerchantParameters = merchantParameters,
            Signature = signature,
            Order = order
        };
    }

    public string? DecodeOrder(string merchantParameters)
    {
        try
        {
            string json = Encoding.UTF8.GetString(FromBase64UrlSafe(merchantParameters));
            using JsonDocument document = JsonDocument.Parse(json);

            string order = GetString(document.RootElement, "Ds_Order");

            return string.IsNullOrWhiteSpace(order) ? null : order;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redsys order could not be decoded.");

            return null;
        }
    }

    public RedsysNotificationDto? VerifySignature(FestivalCredentials credentials, string order, string merchantParameters, string signature)
    {
        if (string.IsNullOrWhiteSpace(merchantParameters) || string.IsNullOrWhiteSpace(signature))
        {
            return null;
        }

        try
        {
            string json = Encoding.UTF8.GetString(FromBase64UrlSafe(merchantParameters));
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            // Redsys signs the Ds_MerchantParameters value exactly as it was sent.
            string expected = Sign(order, merchantParameters, credentials.RedsysSecretKey);
            byte[] expectedBytes = FromBase64UrlSafe(expected);
            byte[] receivedBytes = FromBase64UrlSafe(signature);

            if (!CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes))
            {
                _logger.LogWarning("Redsys signature mismatch for order {Order}.", order);

                return null;
            }

            return new RedsysNotificationDto
            {
                Order = order,
                ResponseCode = int.TryParse(GetString(root, "Ds_Response"), out int code) ? code : -1,
                AmountInCents = long.TryParse(GetString(root, "Ds_Amount"), out long amount) ? amount : 0,
                AuthorisationCode = GetString(root, "Ds_AuthorisationCode").Trim()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redsys notification could not be verified.");

            return null;
        }
    }

    private static string Sign(string order, string payload, string secretKeyBase64)
    {
        byte[] secretKey = Convert.FromBase64String(secretKeyBase64);

        using TripleDES tripleDes = TripleDES.Create();
        tripleDes.Mode = CipherMode.CBC;
        tripleDes.Padding = PaddingMode.Zeros;
        tripleDes.Key = secretKey;
        tripleDes.IV = new byte[8];

        byte[] orderBytes = Encoding.UTF8.GetBytes(order);
        byte[] derivedKey = tripleDes.CreateEncryptor().TransformFinalBlock(orderBytes, 0, orderBytes.Length);

        using HMACSHA256 hmac = new(derivedKey);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return Convert.ToBase64String(hash);
    }

    private static byte[] FromBase64UrlSafe(string value)
    {
        string normalized = value.Replace('-', '+').Replace('_', '/');
        int padding = normalized.Length % 4;

        if (padding == 2)
        {
            normalized += "==";
        }
        else if (padding == 3)
        {
            normalized += "=";
        }

        return Convert.FromBase64String(normalized);
    }

    private static string GetString(JsonElement root, string property)
    {
        return root.TryGetProperty(property, out JsonElement element) ? element.GetString() ?? string.Empty : string.Empty;
    }
}