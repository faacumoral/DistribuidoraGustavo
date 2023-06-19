using Blazored.SessionStorage;
using DistribuidoraGustavo.Interfaces.Models;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace DistribuidoraGustavo.App.Utils;

public static class Extensions
{
    private const string JWT_SESSIONSTORAGE_KEY = "DistGustavo_JWT";
    private const string USER_SESSIONSTORAGE_KEY = "DistDigus_UserLog";

    public static async Task Set<T>(this ISessionStorageService sessionStorage, string key, T value)
    {
        if (value == null) return;

        await sessionStorage.SetItemAsync(key, JsonSerializer.Serialize(value));
    }

    public static async Task<T> Get<T>(this ISessionStorageService sessionStorage, string key)
    {
        var json = await sessionStorage.GetStringAsync(key);
        return string.IsNullOrEmpty(json) ?
            default(T) :
            JsonSerializer.Deserialize<T>(json);
    }

    #region Session storage shortcuts
    public static async Task SetUser(this ISessionStorageService sessionStorage, UserModel user)
       => await sessionStorage.Set(USER_SESSIONSTORAGE_KEY, user);

    public static async Task<UserModel> GetUser(this ISessionStorageService sessionStorage)
        => await sessionStorage.Get<UserModel>(USER_SESSIONSTORAGE_KEY);

    public static async Task SetStringAsync(this ISessionStorageService sessionStorage, string key, string value)
        => await sessionStorage.SetItemAsync(key, value);

    public static async Task<string> GetStringAsync(this ISessionStorageService sessionStorage, string key)
        => await sessionStorage.GetItemAsync<string>(key);

    public static async Task SetJwt(this ISessionStorageService sessionStorage, string jwt)
        => await sessionStorage.SetStringAsync(JWT_SESSIONSTORAGE_KEY, jwt);

    public static async Task<string> GetJwt(this ISessionStorageService sessionStorage)
        => await sessionStorage.GetItemAsync<string>(JWT_SESSIONSTORAGE_KEY);

    public static async Task<bool> ExistsJwt(this ISessionStorageService sessionStorage)
        => !string.IsNullOrEmpty(await sessionStorage.GetJwt());
    #endregion

    public static void Navigate(this NavigationManager navigation, Views view)
        => navigation.NavigateTo(view.ToString());

    public static string ToShortString(this string str) => str.Length > 20 ? str?[..20] : str;
}
