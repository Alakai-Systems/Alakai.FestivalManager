namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IRedsysGateway
{
    RedsysPaymentFormDto BuildPaymentForm(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string? urlOk = null, string? urlKo = null);

    /// <summary>Decodifica el Order de un merchantParameters SIN validar la firma (no necesita clave).</summary>
    string? DecodeOrder(string merchantParameters);

    /// <summary>Valida la firma usando la clave del festival correspondiente y devuelve los datos de la notificacion.</summary>
    RedsysNotificationDto? VerifySignature(FestivalCredentials credentials, string order, string merchantParameters, string signature);

    /// <summary>Envia una peticion de reembolso (total o parcial) a la API REST de Redsys para un pago ya confirmado.</summary>
    Task<RedsysRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string order, long amountInCents, CancellationToken cancellationToken = default);
}