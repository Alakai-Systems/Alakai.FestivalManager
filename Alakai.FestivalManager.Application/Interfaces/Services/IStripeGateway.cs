namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IStripeGateway
{
    /// <summary>Crea una sesion de Stripe Checkout (pagina alojada) para un cobro puntual.</summary>
    Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string urlOk, string urlKo, CancellationToken cancellationToken = default);

    /// <summary>Lee el Order (ClientReferenceId) de un evento de webhook SIN validar la firma (no necesita clave -- solo para saber que registro/festival corresponde y con que secreto verificar despues).</summary>
    string? PeekOrder(string payload);

    /// <summary>Valida la firma del webhook con el secreto del festival correspondiente y devuelve los datos del evento si es un checkout.session.completed.</summary>
    StripeNotificationDto? VerifyAndParseEvent(FestivalCredentials credentials, string payload, string signatureHeader);

    /// <summary>Reembolsa (total o parcialmente) un PaymentIntent ya cobrado.</summary>
    Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default);
}