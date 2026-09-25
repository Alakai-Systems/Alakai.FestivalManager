using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Application.Features.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IRedsysGateway _redsysGateway;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ITicketService _ticketService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IStripeGateway _stripeGateway;

    public PaymentService(IRegistrationRepository registrationRepository, IRedsysGateway redsysGateway,
        IEmailNotificationService emailNotificationService, ITicketService ticketService, ILogger<PaymentService> logger,
        IStripeGateway stripeGateway)
    {
        _registrationRepository = registrationRepository;
        _redsysGateway = redsysGateway;
        _emailNotificationService = emailNotificationService;
        _ticketService = ticketService;
        _logger = logger;
        _stripeGateway = stripeGateway;
    }

    public async Task<ApiResponse<RedsysPaymentFormDto>> CreatePaymentSessionAsync(CreatePaymentSessionCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<RedsysPaymentFormDto> { Success = false, Data = null, Errors = ["Registration not found."], Message = "Payment session failed" };
        }

        if (registration.PaymentStatus == PaymentStatus.Paid)
        {
            return new ApiResponse<RedsysPaymentFormDto> { Success = false, Data = null, Errors = ["This registration is already paid."], Message = "Payment session failed" };
        }

        if (registration.FinalPrice <= 0)
        {
            return new ApiResponse<RedsysPaymentFormDto> { Success = false, Data = null, Errors = ["There is no pending amount to pay."], Message = "Payment session failed" };
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            return new ApiResponse<RedsysPaymentFormDto> { Success = false, Data = null, Errors = ["Payment configuration missing for this festival."], Message = "Payment session failed" };
        }

        string order = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Random.Shared.Next(10, 100).ToString();
        long amountInCents = command.AmountOverride.HasValue
            ? (long)Math.Round(command.AmountOverride.Value * 100m, MidpointRounding.AwayFromZero)
            : (long)Math.Round(registration.FinalPrice * 100m, MidpointRounding.AwayFromZero);

        if (amountInCents <= 0)
        {
            return new ApiResponse<RedsysPaymentFormDto> { Success = false, Data = null, Errors = ["There is no pending amount to pay."], Message = "Payment session failed" };
        }

        // Si ya estaba "PartiallyPaid" (primer tramo de un Split ya pagado), se
        // queda asi mientras se genera el formulario del segundo tramo -- antes
        // esto se sobreescribia a "Pending" con solo abrir el formulario, y si
        // el usuario lo cerraba sin pagar, el panel pasaba a mostrar el 100%
        // como pendiente aunque el primer tramo ya estuviera cobrado.
        if (registration.PaymentStatus != PaymentStatus.PartiallyPaid)
        {
            registration.PaymentStatus = PaymentStatus.Pending;
        }

        registration.PaymentReference = order;
        registration.SetUpdated();

        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        RedsysPaymentFormDto form = _redsysGateway.BuildPaymentForm(credentials, order, amountInCents, "Festival registration", command.UrlOk, command.UrlKo);

        _logger.LogInformation("Redsys payment session created. Order {Order}, registration {RegistrationId}, amount {Amount} cents.", order, registration.Id, amountInCents);

        return new ApiResponse<RedsysPaymentFormDto> { Success = true, Data = form, Errors = [], Message = "Payment session created" };
    }

    public async Task<bool> ProcessRedsysReturnAsync(string merchantParameters, CancellationToken cancellationToken = default)
    {
        try
        {
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(
                merchantParameters.Replace('-', '+').Replace('_', '/').Replace(' ', '+')));

            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
            System.Text.Json.JsonElement root = doc.RootElement;

            string order = root.TryGetProperty("Ds_Order", out System.Text.Json.JsonElement orderEl)
                ? orderEl.GetString() ?? string.Empty : string.Empty;

            string responseCodeStr = root.TryGetProperty("Ds_Response", out System.Text.Json.JsonElement respEl)
                ? respEl.GetString() ?? "-1" : "-1";

            string authCode = root.TryGetProperty("Ds_AuthorisationCode", out System.Text.Json.JsonElement authEl)
                ? (authEl.GetString() ?? string.Empty).Trim() : string.Empty;

            if (string.IsNullOrEmpty(order)) return false;

            int responseCode = int.TryParse(responseCodeStr, out int rc) ? rc : -1;
            bool isApproved = responseCode >= 0 && responseCode <= 99;

            Registration? registration = await _registrationRepository.GetByOrderAsync(order, cancellationToken);
            if (registration is null) return false;
            if (registration.PaymentStatus == PaymentStatus.Paid) return true;
            if (registration.PaymentStatus == PaymentStatus.PartiallyPaid) return true;

            bool becameFullyPaid = false;

            if (isApproved)
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    string entry = $"{order}:{authCode}";
                    registration.PaymentAuthCodes = string.IsNullOrEmpty(registration.PaymentAuthCodes)
                        ? entry
                        : registration.PaymentAuthCodes + "|" + entry;
                }

                registration.PaymentPlatformUsed = PaymentPlatform.Redsys;

                if (registration.PaymentPlan == PaymentPlan.SplitFiftyFifty && registration.AmountPaid == 0m)
                {
                    registration.PaymentStatus = PaymentStatus.PartiallyPaid;
                    registration.AmountPaid = Math.Round(registration.FinalPrice * 0.5m, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    registration.PaymentStatus = PaymentStatus.Paid;
                    registration.PaidAt = DateTime.UtcNow;
                    registration.AmountPaid = registration.FinalPrice;
                    registration.Status = RegistrationStatus.Confirmed;
                    becameFullyPaid = true;
                }
                _logger.LogInformation("Redsys return confirmed. Order {Order}.", order);
            }
            else
            {
                registration.PaymentStatus = PaymentStatus.Failed;
            }

            registration.SetUpdated();
            _registrationRepository.Update(registration);
            await _registrationRepository.SaveChangesAsync(cancellationToken);

            if (becameFullyPaid)
            {
                try
                {
                    await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
                }
                catch (Exception ticketEx)
                {
                    _logger.LogWarning(ticketEx, "Could not generate the ticket for registration {RegistrationId}; the payment confirmation email will still be sent.", registration.Id);
                }
            }

            if (isApproved)
            {
                await _emailNotificationService.CreateAndSendEmailAsync(
                    EmailTemplateKey.PaymentConfirmed, registration.Id, cancellationToken);
            }

            return isApproved;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redsys return processing failed.");
            return false;
        }
    }

    public Task<ApiResponse<RedsysPaymentFormDto>> CreatePaymentSessionAsync(CreatePaymentSessionCommand command, string? urlOk, string? urlKo, CancellationToken cancellationToken = default)
    {
        command.UrlOk = urlOk;
        command.UrlKo = urlKo;

        return CreatePaymentSessionAsync(command, cancellationToken);
    }

    public async Task<bool> ProcessRedsysNotificationAsync(string merchantParameters, string signature, CancellationToken cancellationToken = default)
    {
        string? order = _redsysGateway.DecodeOrder(merchantParameters);

        if (order is null)
        {
            _logger.LogWarning("Redsys notification rejected: could not decode order.");

            return false;
        }

        Registration? registration = await _registrationRepository.GetByPaymentReferenceAsync(order, cancellationToken);

        if (registration is null)
        {
            _logger.LogWarning("Redsys notification for unknown order {Order}.", order);

            return false;
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            _logger.LogWarning("Redsys notification for order {Order} has no festival credentials configured.", order);

            return false;
        }

        RedsysNotificationDto? notification = _redsysGateway.VerifySignature(credentials, order, merchantParameters, signature);

        if (notification is null)
        {
            _logger.LogWarning("Redsys notification rejected: invalid signature or payload.");

            return false;
        }

        if (registration.PaymentStatus == PaymentStatus.Paid)
        {
            return true;
        }

        bool becameFullyPaid = false;

        if (notification.IsApproved)
        {
            if (registration.PaymentPlan == PaymentPlan.SplitFiftyFifty && registration.AmountPaid == 0m)
            {
                decimal paid = Math.Round(registration.FinalPrice * 0.5m, 2, MidpointRounding.AwayFromZero);
                registration.PaymentStatus = PaymentStatus.PartiallyPaid;
                registration.AmountPaid = paid;
            }
            else
            {
                registration.PaymentStatus = PaymentStatus.Paid;
                registration.PaidAt = DateTime.UtcNow;
                registration.AmountPaid = registration.FinalPrice;
                registration.Status = RegistrationStatus.Confirmed;
                becameFullyPaid = true;
            }

            if (!string.IsNullOrEmpty(notification.AuthorisationCode))
            {
                string entry = $"{notification.Order}:{notification.AuthorisationCode}";
                registration.PaymentAuthCodes = string.IsNullOrEmpty(registration.PaymentAuthCodes)
                    ? entry
                    : registration.PaymentAuthCodes + "|" + entry;
            }

            registration.PaymentPlatformUsed = PaymentPlatform.Redsys;

            _logger.LogInformation("Redsys payment approved. Order {Order}, auth code {AuthCode}.", notification.Order, notification.AuthorisationCode);
        }
        else
        {
            registration.PaymentStatus = PaymentStatus.Failed;
            _logger.LogInformation("Redsys payment denied. Order {Order}, response code {Code}.", notification.Order, notification.ResponseCode);
        }

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        if (becameFullyPaid)
        {
            try
            {
                await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
            }
            catch (Exception ticketEx)
            {
                _logger.LogWarning(ticketEx, "Could not generate the ticket for registration {RegistrationId}; the payment confirmation email will still be sent.", registration.Id);
            }
        }

        if (notification.IsApproved)
        {
            await _emailNotificationService.CreateAndSendEmailAsync(
                EmailTemplateKey.PaymentConfirmed, registration.Id, cancellationToken);
        }

        return true;
    }

    public async Task<ApiResponse<bool>> RefundRegistrationAsync(RefundRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["The refund amount must be greater than zero."], Message = "Refund failed" };
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Registration not found."], Message = "Refund failed" };
        }

        decimal alreadyRefunded = registration.RefundedAmount;
        decimal refundable = registration.AmountPaid - alreadyRefunded;

        if (command.Amount > refundable)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = [$"The refund amount exceeds the amount available to refund ({refundable:0.00})."], Message = "Refund failed" };
        }

        // Todas las entradas "order:identificador" guardadas en PaymentAuthCodes, en el
        // orden en que se cobraron (puede haber mas de una, p.ej. en un plan de pago
        // dividido en dos cobros). Para Redsys el identificador es el AuthorisationCode
        // (no se usa para reembolsar, solo se reembolsa contra el Order); para Stripe el
        // identificador ES el PaymentIntentId que hace falta para reembolsar.
        //
        // Antes esto solo miraba la ULTIMA entrada, asi que un reembolso que no cupiera
        // en el cobro mas reciente fallaba aunque la inscripcion, en conjunto, tuviera
        // saldo de sobra repartido entre varios cobros. Ahora se recorren todas: si el
        // importe pedido no cabe en un cobro (porque ya se reembolso parte de el, o
        // porque el cobro es mas pequeno que lo pedido), se sigue con el siguiente hasta
        // completar el importe o agotar los cobros disponibles.
        List<(string Order, string? Identifier)> paymentEntries = (registration.PaymentAuthCodes ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(entry =>
            {
                string[] parts = entry.Split(':');
                return (Order: parts[0], Identifier: parts.Length > 1 ? parts[1] : (string?)null);
            })
            .ToList();

        if (paymentEntries.Count == 0)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No payment is on record for this registration."], Message = "Refund failed" };
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Payment configuration missing for this festival."], Message = "Refund failed" };
        }

        long amountInCents = (long)Math.Round(command.Amount * 100m, MidpointRounding.AwayFromZero);

        // Nulo (inscripciones anteriores a esta funcionalidad) se trata como Redsys,
        // que era la unica plataforma que existia hasta ahora.
        PaymentPlatform platform = registration.PaymentPlatformUsed ?? PaymentPlatform.Redsys;

        bool refundSuccess;
        string refundMessage;
        string platformLabel;
        decimal actuallyRefundedNow;
        List<string> touchedPayments = [];

        if (platform == PaymentPlatform.Stripe)
        {
            platformLabel = "Stripe";

            List<(string Order, string? Identifier)> stripeEntries = paymentEntries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Identifier))
                .ToList();

            if (stripeEntries.Count == 0)
            {
                return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No Stripe payment intent is on record for this registration."], Message = "Refund failed" };
            }

            long remainingCents = amountInCents;
            List<string> failureMessages = [];

            foreach ((string order, string? identifier) in stripeEntries)
            {
                if (remainingCents <= 0)
                {
                    break;
                }

                long refundableHereCents;

                try
                {
                    refundableHereCents = await _stripeGateway.GetRefundableAmountInCentsAsync(credentials, identifier!, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read the refundable amount for Stripe payment intent {PaymentIntentId} (registration {RegistrationId}).", identifier, registration.Id);
                    failureMessages.Add($"No se pudo consultar Stripe para {order}: {ex.Message}");
                    continue;
                }

                long amountHereCents = Math.Min(remainingCents, refundableHereCents);

                if (amountHereCents <= 0)
                {
                    continue;
                }

                StripeRefundResultDto stripeResult = await _stripeGateway.SendRefundAsync(credentials, identifier!, amountHereCents, cancellationToken);

                if (!stripeResult.Success)
                {
                    _logger.LogWarning("Stripe refund rejected for registration {RegistrationId}, payment intent {PaymentIntentId}: {Message}", registration.Id, identifier, stripeResult.Message);
                    failureMessages.Add(stripeResult.Message);
                    continue;
                }

                remainingCents -= amountHereCents;
                touchedPayments.Add($"{amountHereCents / 100m:0.00} contra {order}");
            }

            actuallyRefundedNow = touchedPayments.Count == 0
                ? 0
                : (amountInCents - remainingCents) / 100m;

            refundSuccess = actuallyRefundedNow > 0;
            refundMessage = !refundSuccess
                ? (failureMessages.Count > 0 ? string.Join(" ", failureMessages) : "No queda importe reembolsable en los pagos de Stripe registrados para esta inscripcion.")
                : remainingCents > 0
                    ? $"Reembolsados {actuallyRefundedNow:0.00} de los {command.Amount:0.00} solicitados (no quedaba mas disponible en los pagos registrados)."
                    : "Reembolso confirmado por Stripe.";
        }
        else
        {
            platformLabel = "Redsys";

            string? lastError = null;

            foreach ((string order, string? _) in paymentEntries)
            {
                RedsysRefundResultDto redsysResult = await _redsysGateway.SendRefundAsync(credentials, order, amountInCents, cancellationToken);

                if (redsysResult.Success)
                {
                    touchedPayments.Add($"{command.Amount:0.00} contra {order}");
                    lastError = null;
                    break;
                }

                lastError = redsysResult.Message;
            }

            actuallyRefundedNow = touchedPayments.Count > 0 ? command.Amount : 0;
            refundSuccess = actuallyRefundedNow > 0;
            refundMessage = refundSuccess
                ? "Reembolso confirmado por Redsys."
                : (paymentEntries.Count > 1
                    ? $"Redsys rechazo el reembolso contra los {paymentEntries.Count} pagos registrados. Ultimo motivo: {lastError}"
                    : lastError ?? "Redsys rejected the refund.");
        }

        if (!refundSuccess)
        {
            _logger.LogWarning("{Platform} refund rejected for registration {RegistrationId}: {Message}", platformLabel, registration.Id, refundMessage);

            return new ApiResponse<bool> { Success = false, Data = false, Errors = [refundMessage], Message = "Refund failed" };
        }

        registration.RefundedAmount = alreadyRefunded + actuallyRefundedNow;

        // Un reembolso NO debe reabrir el registro para volver a cobrar: es dinero
        // que se devuelve a proposito, no una deuda pendiente. Por eso ya NO se toca
        // PaymentStatus aqui -- se deja tal cual estaba antes del reembolso:
        //   - Si estaba "Paid" (pago completo, o un Split ya completado), se queda
        //     "Paid": no aparece nada pendiente ni el boton de pago.
        //   - Si estaba "PartiallyPaid" (Split con solo el primer 50% pagado), se
        //     queda "PartiallyPaid": el segundo 50% sigue pendiente de pago, se
        //     reembolse o no ese primer tramo -- es un tramo aparte.

        string note = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] Reembolso de {actuallyRefundedNow:0.00} via {platformLabel}: {string.Join(", ", touchedPayments)}." +
            (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Motivo: {command.Reason}") +
            (actuallyRefundedNow < command.Amount ? $" ATENCION: solo se pudo reembolsar {actuallyRefundedNow:0.00} de los {command.Amount:0.00} solicitados -- revisar manualmente el resto." : string.Empty);
        registration.InternalNotes = string.IsNullOrWhiteSpace(registration.InternalNotes)
            ? note
            : registration.InternalNotes + "\n" + note;

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{Platform} refund confirmed for registration {RegistrationId}, amount {Amount} cents: {Details}.", platformLabel, registration.Id, (long)Math.Round(actuallyRefundedNow * 100m, MidpointRounding.AwayFromZero), string.Join(", ", touchedPayments));

        return new ApiResponse<bool> { Success = true, Data = true, Errors = [], Message = refundMessage };
    }

    // ── Stripe ──────────────────────────────────────────────────────────────
    // Checkout alojado (Stripe redirige el ya mismo a su propia pagina de pago,
    // sin necesidad de firmar un formulario como con Redsys). El mismo Order que
    // usa Redsys se manda como ClientReferenceId, para poder recuperar la
    // inscripcion en el webhook sin ambiguedad.

    public async Task<ApiResponse<StripeCheckoutSessionDto>> CreateStripeCheckoutSessionAsync(CreatePaymentSessionCommand command, string? urlOk, string? urlKo, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["Registration not found."], Message = "Payment session failed" };
        }

        if (registration.PaymentStatus == PaymentStatus.Paid)
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["This registration is already paid."], Message = "Payment session failed" };
        }

        if (registration.FinalPrice <= 0)
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["There is no pending amount to pay."], Message = "Payment session failed" };
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null || string.IsNullOrWhiteSpace(credentials.StripeSecretKey))
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["Stripe is not configured for this festival."], Message = "Payment session failed" };
        }

        string order = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Random.Shared.Next(10, 100).ToString();
        long amountInCents = command.AmountOverride.HasValue
            ? (long)Math.Round(command.AmountOverride.Value * 100m, MidpointRounding.AwayFromZero)
            : (long)Math.Round(registration.FinalPrice * 100m, MidpointRounding.AwayFromZero);

        if (amountInCents <= 0)
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["There is no pending amount to pay."], Message = "Payment session failed" };
        }

        string effectiveUrlOk = urlOk ?? command.UrlOk ?? string.Empty;
        string effectiveUrlKo = urlKo ?? command.UrlKo ?? string.Empty;

        if (string.IsNullOrWhiteSpace(effectiveUrlOk) || string.IsNullOrWhiteSpace(effectiveUrlKo))
        {
            return new ApiResponse<StripeCheckoutSessionDto> { Success = false, Data = null, Errors = ["Missing return URLs for the Stripe checkout session."], Message = "Payment session failed" };
        }

        // Mismo caso que en Redsys (ver CreatePaymentSessionAsync mas arriba): si
        // ya estaba "PartiallyPaid", se queda asi al generar la sesion de Stripe
        // para el tramo restante, en vez de sobreescribirse a "Pending" con solo
        // abrir el checkout.
        if (registration.PaymentStatus != PaymentStatus.PartiallyPaid)
        {
            registration.PaymentStatus = PaymentStatus.Pending;
        }

        registration.PaymentReference = order;
        registration.SetUpdated();

        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        StripeCheckoutSessionDto session = await _stripeGateway.CreateCheckoutSessionAsync(
            credentials, order, amountInCents, "Festival registration", effectiveUrlOk, effectiveUrlKo, cancellationToken);

        _logger.LogInformation("Stripe payment session created. Order {Order}, registration {RegistrationId}, amount {Amount} cents.", order, registration.Id, amountInCents);

        return new ApiResponse<StripeCheckoutSessionDto> { Success = true, Data = session, Errors = [], Message = "Payment session created" };
    }

    public async Task<bool> ProcessStripeWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
    {
        string? order = _stripeGateway.PeekOrder(payload);

        if (order is null)
        {
            _logger.LogWarning("Stripe webhook rejected: could not read the order from the payload.");

            return false;
        }

        Registration? registration = await _registrationRepository.GetByPaymentReferenceAsync(order, cancellationToken);

        if (registration is null)
        {
            _logger.LogWarning("Stripe webhook for unknown order {Order}.", order);

            return false;
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            _logger.LogWarning("Stripe webhook for order {Order} has no festival credentials configured.", order);

            return false;
        }

        StripeNotificationDto? notification = _stripeGateway.VerifyAndParseEvent(credentials, payload, signatureHeader);

        if (notification is null)
        {
            _logger.LogWarning("Stripe webhook rejected: invalid signature, payload, or not a completed checkout session.");

            return false;
        }

        if (registration.PaymentStatus == PaymentStatus.Paid)
        {
            return true;
        }

        bool becameFullyPaid = false;

        if (notification.IsApproved)
        {
            if (registration.PaymentPlan == PaymentPlan.SplitFiftyFifty && registration.AmountPaid == 0m)
            {
                decimal paid = Math.Round(registration.FinalPrice * 0.5m, 2, MidpointRounding.AwayFromZero);
                registration.PaymentStatus = PaymentStatus.PartiallyPaid;
                registration.AmountPaid = paid;
            }
            else
            {
                registration.PaymentStatus = PaymentStatus.Paid;
                registration.PaidAt = DateTime.UtcNow;
                registration.AmountPaid = registration.FinalPrice;
                registration.Status = RegistrationStatus.Confirmed;
                becameFullyPaid = true;
            }

            if (!string.IsNullOrEmpty(notification.PaymentIntentId))
            {
                string entry = $"{notification.Order}:{notification.PaymentIntentId}";
                registration.PaymentAuthCodes = string.IsNullOrEmpty(registration.PaymentAuthCodes)
                    ? entry
                    : registration.PaymentAuthCodes + "|" + entry;
            }

            registration.PaymentPlatformUsed = PaymentPlatform.Stripe;

            _logger.LogInformation("Stripe payment approved. Order {Order}, payment intent {PaymentIntentId}.", notification.Order, notification.PaymentIntentId);
        }
        else
        {
            registration.PaymentStatus = PaymentStatus.Failed;
            _logger.LogInformation("Stripe payment not approved. Order {Order}.", notification.Order);
        }

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        if (becameFullyPaid)
        {
            try
            {
                await _ticketService.EnsureTicketGeneratedAsync(registration.Id, cancellationToken);
            }
            catch (Exception ticketEx)
            {
                _logger.LogWarning(ticketEx, "Could not generate the ticket for registration {RegistrationId}; the payment confirmation email will still be sent.", registration.Id);
            }
        }

        if (notification.IsApproved)
        {
            await _emailNotificationService.CreateAndSendEmailAsync(
                EmailTemplateKey.PaymentConfirmed, registration.Id, cancellationToken);
        }

        return true;
    }
}
