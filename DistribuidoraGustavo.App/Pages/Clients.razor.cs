using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;


public class ClientsBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }


    protected IList<ClientModel> Clients { get; set; } = new List<ClientModel>();

    public bool Searching { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Searching = true;

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(ApiRequest.BuildGet("Clients"));

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        Searching = false;

        await base.OnInitializedAsync();
    }

    protected void EditClient(ClientModel client)
    {
        NavigationManager.NavigateTo(Views.AddOrEditClient.ToString() + "/" + client.ClientId);
    }

    protected void AddNewClient()
    {
        NavigationManager.NavigateTo(Views.AddOrEditClient.ToString());
    }
}
