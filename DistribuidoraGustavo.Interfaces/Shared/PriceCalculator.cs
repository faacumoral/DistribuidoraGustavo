namespace DistribuidoraGustavo.Interfaces.Shared;

public static class PriceCalculator
{
    /// <summary>
    /// Price of a product in a price list: base price plus the list percent, rounded up to an integer
    /// </summary>
    public static decimal Calculate(decimal basePrice, decimal percent)
        => Math.Ceiling(basePrice + (basePrice * percent / 100));
}
