using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;

public class PricesState
{
    public int PriceListSelected { get; set; } = 0;
    public bool Processing { get; set; } = false;
    public string ProductFilter { get; set; }
}


public class PricesBase : ComponentBase
{
    #region DI
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    #endregion

    public SearchInput SearchInput { get; set; }
    public PricesState State { get; set; } = new();
    public Alerts Alert { get; set; }
    public IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();
    public IList<PricedProductModel> Products { get; set; } = new List<PricedProductModel>();

    public HiddenFileInput FileInput { get; set; }

    protected override async Task OnInitializedAsync()
    {
        State.Processing = true;

        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(ApiRequest.BuildGet("PriceLists"));

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk;

        State.Processing = false;

        await base.OnInitializedAsync();
    }

    protected void PriceListChanged(int priceListId)
    {
        State.PriceListSelected = priceListId;
        SearchProducts();
    }

    protected async void SearchProducts()
    {
        State.Processing = true;
        StateHasChanged();

        var url = $"products/prices?filter={State.ProductFilter}&priceListId={State.PriceListSelected}";

         var productsResult = await ApiClient.Send<ListResult<PricedProductModel>>(ApiRequest.BuildGet(url));

        if (productsResult.Success)
            Products = productsResult.ResultOk;
        else
            Alert.ShowError(productsResult.ResultError.FriendlyErrorMessage);

        State.Processing = false;

        await SearchInput.Focus();

        StateHasChanged();
    }

    protected async void UploadFile(MultipartFormDataContent file)
    {
        await ApiClient.SendFormData<BoolResult>("PriceLists/uploadFile", file);
        file.Dispose();
    }

}
