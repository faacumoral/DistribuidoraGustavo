using Blazored.SessionStorage;
using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Responses;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;

public class LoginBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }


    protected bool requestSent = false;

    protected Credentials credentials = new();
    
    protected DTOResult<LoginResponse> loginResponse;

    protected async Task Login()
    {
        var apiRequest = ApiRequest.BuildPost("Users/Login", credentials, false);
        requestSent = true;
        loginResponse = await ApiClient.Send<DTOResult<LoginResponse>>(apiRequest);
        requestSent = false;
        if (loginResponse.Success)
        {
            await SessionStorage.SetJwt(loginResponse.ResultOk.Jwt);
            await SessionStorage.SetUser(loginResponse.ResultOk.User);
            NavigationManager?.NavigateTo(Views.Home.ToString());
        }
    }
}

public class Credentials
{ 
    public string Username { get; set; }
    public string Password { get; set; }
}
