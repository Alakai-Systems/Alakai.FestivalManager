using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Alakai.FestivalManager.Infrastructure.Payments;

public class StripeGateway : IStripeGateway
{
    private readonly ILogger<StripeGateway> _logger;

    public StripeGateway(ILogger<StripeGateway> logger)
    {
        _logger = logger;
    }

    public async Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string urlOk, string urlKo, CancellationToken cancellationToken = default)
    {
        SessionCreateOptions options = new()
        {
            Mode = "payment",
            ClientReferenceId = order,
            SuccessUrl = urlOk,
            CancelUrl = urlKo,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = string.IsNullOrWhiteSpace(credentials.Currency) ? "eur" : credentials.Currency.ToLowerInvariant(),
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = productDescription
                        }
                    }
                }
            }
        };

        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        SessionService service = new();
        Session session = await service.CreateAsync(options, requestOptions, cancellationToken);

        _logger.LogInformation("Stripe checkout session created. Order {Order}, session {SessionId}.", order, session.Id);

        return new StripeCheckoutSessionDto { Url = session.Url, SessionId = session.Id };
    }

    public string? PeekOrder(string payload)
    {
        try
        {
            Event stripeEvent = EventUtility.ParseEvent(payload);

            if (stripeEvent.Data.Object is Session session && !string.IsNullOrWhiteSpace(session.ClientReferenceId))
            {
                return session.ClientReferenceId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook payload could not be peeked for the order.");

            return null;
        }
    }

    public StripeNotificationDto? VerifyAndParseEvent(FestivalCredentials credentials, string payload, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(credentials.StripeWebhookSecret) || string.IsNullOrWhiteSpace(signatureHeader))
        {
            return null;
        }

        try
        {
            Event stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, credentials.StripeWebhookSecret);

            if (stripeEvent.Type != "checkout.session.completed" || stripeEvent.Data.Object is not Session session)
            {
                return null;
            }

            return new StripeNotificationDto
            {
                Order = session.ClientReferenceId ?? string.Empty,
                IsApproved = session.PaymentStatus == "paid",
                AmountInCents = session.AmountTotal ?? 0,
                PaymentIntentId = session.PaymentIntentId
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed.");

            return null;
        }
    }

    public async Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default)
    {
        RefundCreateOptions options = new()
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents
        };

        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        RefundService service = new();

        try
        {
            Refund refund = await service.CreateAsync(options, requestOptions, cancellationToken);

            bool success = refund.Status == "succeeded" || refund.Status == "pending";

            _logger.LogInformation("Stripe refund created. Payment intent {PaymentIntentId}, refund {RefundId}, status {Status}.", paymentIntentId, refund.Id, refund.Status);

            return new StripeRefundResultDto
            {
                Success = success,
                ResponseCode = refund.Status,
                Message = success ? "Reembolso aceptado por Stripe." : $"Stripe devolvio el estado '{refund.Status}'."
            };
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe refund failed for payment intent {PaymentIntentId}.", paymentIntentId);

            return new StripeRefundResultDto
            {
                Success = false,
                ResponseCode = ex.StripeError?.Code,
                Message = ex.StripeError?.Message ?? "Stripe refund failed."
            };
        }
    }

    /// <summary>
    /// Cuanto le queda reembolsable (en centimos) al cargo de un PaymentIntent ya cobrado.
    /// Necesario para repartir un reembolso entre varios cobros de una misma inscripcion
    /// (p.ej. un plan de pago dividido en dos cargos de Stripe): sin esto, un reembolso
    /// que pida mas de lo que le queda a UN cargo concreto lo rechaza Stripe entero,
    /// aunque la inscripcion en conjunto tenga saldo suficiente repartido entre varios.
    /// </summary>
    public async Task<long> GetRefundableAmountInCentsAsync(FestivalCredentials credentials, string paymentIntentId, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        PaymentIntentService service = new();
        PaymentIntentGetOptions options = new() { Expand = ["latest_charge"] };

        PaymentIntent paymentIntent = await service.GetAsync(paymentIntentId, options, requestOptions, cancellationToken);
        Charge? charge = paymentIntent.LatestCharge;

        if (charge is null)
        {
            return 0;
        }

        long refundable = charge.Amount - charge.AmountRefunded;

        return refundable > 0 ? refundable : 0;
    }
}