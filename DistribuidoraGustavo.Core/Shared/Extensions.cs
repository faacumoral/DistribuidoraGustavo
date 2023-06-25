﻿namespace DistribuidoraGustavo.Core.Shared;

public static class Extensions
{
    public static string? ToShortString(this string str) => str.Length > 20 ? str?[..20] : str;
    public static string ReplaceToken(this string str, string token, string value) => str.Replace("{{" + token+ "}}", value);

}
