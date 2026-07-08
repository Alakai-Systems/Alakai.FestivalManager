namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IRedsysGateway
{
    RedsysPaymentFormDto BuildPaymentForm(string order, long amountInCents, string productDescription, string? urlOk = null, string? urlKo = null);
    RedsysNotificationDto? ValidateNotification(string merchantParameters, string signature);
}
