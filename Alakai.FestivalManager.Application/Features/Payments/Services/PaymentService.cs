using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Application.Features.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IRedsysGateway _redsysGateway;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IRegistrationRepository registrationRepository, IRedsysGateway redsysGateway,
        IEmailNotificationService emailNotificationService, ILogger<PaymentService> logger)
    {
        _registrationRepository = registrationRepository;
        _redsysGateway = redsysGateway;
        _emailNotificationService = emailNotificationService;
        _logger = logger;
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

        string order = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Random.Shared.Next(10, 100).ToString();
        long amountInCents = command.AmountOverride.HasValue
            ? (long)Math.Round(command.AmountOverride.Value * 100m, MidpointRounding.AwayFromZero)
            : (long)Math.Round(registration.FinalPrice * 100m, MidpointRounding.AwayFromZero);

        registration.PaymentStatus = PaymentStatus.Pending;
        registration.PaymentReference = order;
        registration.SetUpdated();

        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        RedsysPaymentFormDto form = _redsysGateway.BuildPaymentForm(order, amountInCents, "Festival registration", command.UrlOk, command.UrlKo);

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

            if (isApproved)
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    string entry = $"{order}:{authCode}";
                    registration.PaymentAuthCodes = string.IsNullOrEmpty(registration.PaymentAuthCodes)
                        ? entry
                        : registration.PaymentAuthCodes + "|" + entry;
                }

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
        RedsysNotificationDto? notification = _redsysGateway.ValidateNotification(merchantParameters, signature);

        if (notification is null)
        {
            _logger.LogWarning("Redsys notification rejected: invalid signature or payload.");

            return false;
        }

        Registration? registration = await _registrationRepository.GetByPaymentReferenceAsync(notification.Order, cancellationToken);

        if (registration is null)
        {
            _logger.LogWarning("Redsys notification for unknown order {Order}.", notification.Order);

            return false;
        }

        if (registration.PaymentStatus == PaymentStatus.Paid)
        {
            return true;
        }

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
            }

            if (!string.IsNullOrEmpty(notification.AuthorisationCode))
            {
                string entry = $"{notification.Order}:{notification.AuthorisationCode}";
                registration.PaymentAuthCodes = string.IsNullOrEmpty(registration.PaymentAuthCodes)
                    ? entry
                    : registration.PaymentAuthCodes + "|" + entry;
            }

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

        if (notification.IsApproved)
        {
            await _emailNotificationService.CreateAndSendEmailAsync(
                EmailTemplateKey.PaymentConfirmed, registration.Id, cancellationToken);
        }

        return true;
    }
}
