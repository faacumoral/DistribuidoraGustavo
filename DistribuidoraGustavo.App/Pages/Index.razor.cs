using Blazored.SessionStorage;
using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Responses;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;

public class IndexBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var existsJwt = await SessionStorage.ExistsJwt();
        if (!existsJwt)
            NavigationManager.Navigate(Views.Login);

        await base.OnInitializedAsync();
    }

    //protected async Task Login()
    //{
    //    var apiRequest = ApiRequest.BuildPost("Users/Login", credentials, false);
    //    requestSent = true;
    //    loginResponse = await ApiClient.Send<DTOResult<LoginResponse>>(apiRequest);
    //    requestSent = false;
    //    if (loginResponse.Success)
    //    {
    //        await SessionStorage.SetJwt(loginResponse.ResultOk.Jwt);
    //        NavigationManager?.NavigateTo(Views.Home.ToString());
    //    }
    //}
}

