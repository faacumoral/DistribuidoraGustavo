using Blazored.SessionStorage;
using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Shared;

public class MainLayoutBase : LayoutComponentBase
{
    //[Inject] ApiClient ApiClient { get; set; }
    [Inject] protected ISessionStorageService SessionStorage { get; set; }
    //[Inject] NavigationManager NavigationManager { get; set; }


    protected UserModel LoggedUser { get; set; }

    protected override async Task OnInitializedAsync()
    {
        LoggedUser = await SessionStorage.GetUser();

        await base.OnInitializedAsync();
    }


}

