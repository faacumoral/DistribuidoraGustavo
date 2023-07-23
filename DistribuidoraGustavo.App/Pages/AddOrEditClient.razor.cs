using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;


public class AddOrEditClientBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }

    [Parameter] public int ClientId { get; set; }

    public Alerts Alert { get; set; }
    public ClientModel Client { get; set; } = new ();

    public IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    public bool Processing { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Processing = true;

        Client.ClientId = ClientId;

        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(ApiRequest.BuildGet("PriceLists"));

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk;

        if (ClientId != 0)
        {
            var clientResult = await ApiClient.Send<DTOResult<ClientModel>>(ApiRequest.BuildGet($"Clients/{ClientId}"));
            if (clientResult.Success)
                Client = clientResult.ResultOk;
        }
        else
        {
            Client.DefaultPriceList = PriceLists.FirstOrDefault();
        }

        Processing = false;

        await base.OnInitializedAsync();
    }

    protected async Task SaveClient()
    {
        var request = ApiRequest.BuildPost("Clients", Client);
        Processing = true;
        var clientResult = await ApiClient.Send<DTOResult<ClientModel>>(request);
        Processing = false;

        if (clientResult.Success)
        {
            NavigationManager.NavigateTo(Views.Clients.ToString());
        }
        else
        {
            Alert.ShowError(clientResult.ResultError.FriendlyErrorMessage);
        }
    }

}
