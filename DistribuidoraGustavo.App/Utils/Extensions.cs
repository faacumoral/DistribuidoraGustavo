using Blazored.SessionStorage;

namespace DistribuidoraGustavo.App.Utils;

public static class Extensions
{
    private const string JWT_SESSIONSTORAGE_KEY = "DistGustavo_JWT";

    public static async Task SetStringAsync(this ISessionStorageService sessionStorage, string key, string value)
        => await sessionStorage.SetItemAsync(key, value);

    public static async Task<string> GetStringAsync(this ISessionStorageService sessionStorage, string key)
        => await sessionStorage.GetItemAsync<string>(key);

    public static async Task SetJwt(this ISessionStorageService sessionStorage, string jwt)
        => await sessionStorage.SetStringAsync(JWT_SESSIONSTORAGE_KEY, jwt);

    public static async Task<string> GetJwt(this ISessionStorageService sessionStorage)
        => await sessionStorage.GetItemAsync<string>(JWT_SESSIONSTORAGE_KEY);

}
