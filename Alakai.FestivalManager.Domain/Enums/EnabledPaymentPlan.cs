namespace Alakai.FestivalManager.Domain.Enums;

/// <summary>
/// Que planes de pago (ver PaymentPlan) puede elegir quien se registra en un
/// festival. Enum de flags DISTINTO de PaymentPlan a proposito: PaymentPlan ya
/// tiene los valores 1/2/3 grabados en produccion (el plan que eligio cada
/// inscripcion existente) y no son potencias de 2, asi que no se puede
/// reutilizar como mascara de bits sin arriesgar esos datos.
/// </summary>
[Flags]
public enum EnabledPaymentPlan
{
    None = 0,
    FullOnline = 1,
    SplitFiftyFifty = 2,
    DeferredTenDays = 4
}