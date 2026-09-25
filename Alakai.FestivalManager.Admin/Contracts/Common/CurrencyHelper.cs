namespace Alakai.FestivalManager.Admin.Contracts.Common;

/// <summary>
/// Simbolo a mostrar para cada divisa soportada (ver el selector de Currency
/// en Payment Settings: EUR/USD/CAD). Se usa en vez de "€" fijo en cualquier
/// importe mostrado al usuario, para que festivales en otra divisa no
/// muestren el simbolo equivocado.
/// </summary>
public static class CurrencyHelper
{
    public static string Symbol(string? currencyCode) => currencyCode?.Trim().ToUpperInvariant() switch
    {
        "USD" => "$",
        "CAD" => "CA$",
        _ => "€"
    };
}