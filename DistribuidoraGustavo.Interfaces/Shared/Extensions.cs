namespace DistribuidoraGustavo.Interfaces.Shared;

public static class Extensions
{
    public static string? ToShortString(this string str) => str.Length > 35 ? str?[..35] : str;
    public static string ReplaceToken(this string str, string token, string value) => str.Replace("{{" + token+ "}}", value);
    
    /// <summary>
    /// cast to Arg timezone
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string DateToString(this DateTime dt) => dt.AddHours(-3).ToString("dd/MM/yyyy HH:mm");
}
