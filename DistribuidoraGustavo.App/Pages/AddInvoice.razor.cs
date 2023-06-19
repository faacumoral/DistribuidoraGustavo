using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Services.Toast;
using DistribuidoraGustavo.App.UIModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;

namespace DistribuidoraGustavo.App.Pages;

public class AddInvoiceState
{
    public int PriceListSelected { get; set; }
    public int ClientSelected { get; set; }
    public string ProductSearch { get; set; }
    public bool SearchingProducts { get; set; } = false;
}

public class AddInvoiceBase : ComponentBase
{
    [Inject] IJSRuntime jsRuntime { get; set; }
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] ToastService toastService { get; set; }


    private System.Timers.Timer timer = default!;

    protected AddInvoiceState State = new();

    protected IList<ProductInvoiceModel> ProductInvoices { get; set; } = new List<ProductInvoiceModel>();
    protected IList<UIProductModel> Products { get; set; } = new List<UIProductModel>();
    protected IList<ClientModel> Clients { get; set; } = new List<ClientModel>();
    protected IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    protected IList<ProductModel> CheckedProducts = new List<ProductModel>();

    private void InitTimer()
    {
        timer = new System.Timers.Timer(2000);
        timer.Elapsed += SearchProducts;
        timer.AutoReset = false;
    }

    protected override async Task OnInitializedAsync()
    {
        toastService.ShowSucces("probando");
        InitTimer();

        var clientsApiRequest = ApiRequest.BuildGet("Clients");
        var priceListsApiRequest = ApiRequest.BuildGet("PriceLists");
        var productsApiRequest = ApiRequest.BuildGet("Products");

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(clientsApiRequest);
        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(priceListsApiRequest);
        var productsResult = await ApiClient.Send<ListResult<UIProductModel>>(productsApiRequest);

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk;

        if (productsResult.Success)
            Products = productsResult.ResultOk;

        await base.OnInitializedAsync();
    }

    private async void SearchProducts(object? source, ElapsedEventArgs e)
    {
        Products = new List<UIProductModel>();

        if (State.ClientSelected == 0 || State.PriceListSelected == 0)
        {
            toastService.ShowSucces("error");

            return;
        }
        State.SearchingProducts = true;

        await InvokeAsync(StateHasChanged);

        var productsApiRequest = ApiRequest.BuildGet($"Products?filter={State.ProductSearch}&priceListId={State.PriceListSelected}");
        var productsResult = await ApiClient.Send<ListResult<UIProductModel>>(productsApiRequest);

        if (productsResult.Success)
            Products = productsResult.ResultOk;

        State.SearchingProducts = false;
        await InvokeAsync(StateHasChanged);

        await jsRuntime.InvokeVoidAsync("FocusElement", "txtSearchProduct");
    }

    protected void WaitAndSearch(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            timer.Stop();
            SearchProducts(null, null);
            return;
        }

        timer.Stop();
        timer.Start();
    }

    protected void ClientChange(int clientId)
    {
        State.ClientSelected = clientId;
        State.PriceListSelected = Clients.FirstOrDefault(c => c.ClientId == clientId)?.DefaultPriceListId ?? 0;
        // seleccionar lista de precio default de ese cliente
        SearchProducts(null, null);
    }

    protected void PriceListChange(int priceListId)
    { 
        State.PriceListSelected = priceListId;
        SearchProducts(null, null);
    }

    protected void AddProducts()
    {
        var checkedProducts = Products.Where(p => p.Checked).ToList();

        foreach (var product in checkedProducts)
        {
            ProductInvoices.Add(new ProductInvoiceModel
            {
                Code = product.Code,
                Name = product.Name,
                ProductId = product.ProductId,
                UnitPrice = product.UnitPrice,
                Amount = product.UnitPrice,
                Quantity = 1
            });
        }
    }


}
