namespace Alakai.FestivalManager.Domain.Enums;

// Que plataformas de pago puede usar quien se registra en este festival.
// Redsys es la unica que existia hasta ahora, por eso es el valor por
// defecto -- para no cambiar el comportamiento de los festivales que ya
// existen. PayPal (4) se deja reservado para una fase posterior.
[Flags]
public enum EnabledPaymentPlatform
{
    None = 0,
    Redsys = 1,
    Stripe = 2,
    PayPal = 4
}