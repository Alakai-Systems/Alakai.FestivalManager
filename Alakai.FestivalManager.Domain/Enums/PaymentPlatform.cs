namespace Alakai.FestivalManager.Domain.Enums;

// Distinto de EnabledPaymentPlatform (la mascara de plataformas habilitadas
// en el festival). Este es el medio de pago REALMENTE usado en cada
// inscripcion concreta -- mismo patron que PaymentPlan frente a
// EnabledPaymentPlan.
public enum PaymentPlatform
{
    Redsys = 1,
    Stripe = 2,
    PayPal = 3
}