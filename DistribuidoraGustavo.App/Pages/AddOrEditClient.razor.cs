using DistribuidoraGustavo.App.Http;
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

    public ClientModel Client { get; set; } = new ();

    public IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    public bool Searching { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Searching = true;

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

        Searching = false;

        await base.OnInitializedAsync();
    }

    protected void SaveClient()
    { 
        
    }

}
