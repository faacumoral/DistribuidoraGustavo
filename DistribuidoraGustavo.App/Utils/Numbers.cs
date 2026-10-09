using System.Globalization;
using System.Text.RegularExpressions;

namespace DistribuidoraGustavo.App.Utils;

public static class Numbers
{
    // 1.234,5 : explicit format so it does not depend on the browser culture
    private static readonly NumberFormatInfo ArFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ","
    };

    private static readonly Regex ThousandsWithDots = new(@"^\d{1,3}(\.\d{3})+$");

    /// <summary>
    /// Parses what a person types as an amount: 1500 | 1500,50 | 1.500 | 1.500,50 | 1500.50
    /// </summary>
    public static bool TryParseAmount(string text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Replace("$", "").Replace(" ", "");

        if (text.Contains(','))
            text = text.Replace(".", "").Replace(",", ".");
        else if (ThousandsWithDots.IsMatch(text))
            text = text.Replace(".", "");

        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)
            && value >= 0;
    }

    public static string ToMoney(this decimal value)
        => "$ " + value.ToString(value % 1 == 0 ? "#,##0" : "#,##0.00", ArFormat);

    public static string ToEditable(this decimal value)
        => value.ToString(value % 1 == 0 ? "0" : "0.00", ArFormat);
}
