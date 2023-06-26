using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.UIModels;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;

namespace DistribuidoraGustavo.App.Pages;

public class AddOrEditInvoiceState
{
    public int PriceListSelected { get; set; }
    public int ClientSelected { get; set; }
    public string ProductSearch { get; set; }
    public bool SearchingProducts { get; set; } = false;
    public bool Saving { get; set; } = false;

}

public class AddOrEditInvoiceBase : ComponentBase
{
    [Inject] IJSRuntime jsRuntime { get; set; }
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    [Parameter]
    public int InvoiceId { get; set; }
    public Alerts Alert { get; set; }

    private System.Timers.Timer timer = default!;

    protected AddOrEditInvoiceState State = new();

    protected IList<ProductInvoiceModel> ProductInvoices { get; set; } = new List<ProductInvoiceModel>();
    protected IList<UIProductModel> Products { get; set; } = new List<UIProductModel>();
    protected IList<ClientModel> Clients { get; set; } = new List<ClientModel>();
    protected IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    private void InitTimer()
    {
        timer = new System.Timers.Timer(2000);
        timer.Elapsed += SearchProducts;
        timer.AutoReset = false;
    }

    protected override async Task OnInitializedAsync()
    {
        InitTimer();

        var clientsApiRequest = ApiRequest.BuildGet("Clients");
        var priceListsApiRequest = ApiRequest.BuildGet("PriceLists");

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(clientsApiRequest);
        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(priceListsApiRequest);

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk;

        if (InvoiceId != 0)
            await LoadInvoice();

        await base.OnInitializedAsync();
    }

    protected async Task LoadInvoice()
    {
        var invoiceRequest = ApiRequest.BuildGet($"Invoices/{InvoiceId}");
        var invoiceResult = await ApiClient.Send<DTOResult<InvoiceModel>>(invoiceRequest);

        if (!invoiceResult.Success)
        {
            NavigationManager.NavigateTo(Views.Invoices.ToString());
            return;
        }

        var invoice = invoiceResult.ResultOk;
        ClientChange(invoice.Client.ClientId);
        ProductInvoices = invoice.Products;
    }

    private async void SearchProducts(object? source, ElapsedEventArgs e)
    {
        Products = new List<UIProductModel>();

        if (State.ClientSelected == 0 || State.PriceListSelected == 0)
        {
            Alert.ShowError("Seleccione un cliente y una lista de precios antes de buscar productos");
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
        State.PriceListSelected = Clients.FirstOrDefault(c => c.ClientId == clientId)?.DefaultPriceList?.PriceListId ?? 0;
        SearchProducts(null, null);
    }

    protected void PriceListChange(int priceListId)
    { 
        State.PriceListSelected = priceListId;
        SearchProducts(null, null);
    }

    protected void ProductValueChanged(ProductInvoiceModel product)
    {
        product.Amount = product.Quantity * product.UnitPrice;
        StateHasChanged();
    }

    protected void RemoveInvoiceProduct(ProductInvoiceModel product)
    {
        ProductInvoices.Remove(product);
    }

    protected void AddProducts()
    {
        var checkedProducts = Products.Where(p => p.Checked).ToList();

        var checkedProductsIds = checkedProducts.Select(cp => cp.ProductId);

        foreach (var product in checkedProducts)
        {
            var productInvoice = ProductInvoices.FirstOrDefault(pi => pi.ProductId == product.ProductId);
            if (productInvoice == null)
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
            else
            {
                productInvoice.Quantity += 1;
                productInvoice.Amount += product.UnitPrice;
            }
        }

    }

    protected async Task SaveInvoice()
    {
        if (State.ClientSelected == 0) 
        {
            Alert.ShowError("Seleccione un cliente antes guardar la factura");
            return;
        }
        if (ProductInvoices.Count == 0) {
            Alert.ShowError("Seleccione al menos un producto antes de guardar la factura");
            return;
        }
        
        var invoice = new InvoiceModel
        {
            Client = Clients.FirstOrDefault(c => c.ClientId == State.ClientSelected),
            PriceList = PriceLists.FirstOrDefault(pl => pl.PriceListId == State.PriceListSelected),
            Products = ProductInvoices,
            InvoiceId = InvoiceId
        };

        State.Saving = true;

        var request = ApiRequest.BuildPost("Invoices", invoice);

        var invoiceResult = await ApiClient.Send<DTOResult<InvoiceModel>>(request);
        State.Saving = false;

        if (invoiceResult.Success)
        {
            Alert.ShowSuccess("Factura guardada correctamente");
            NavigationManager.NavigateTo(Views.Invoices.ToString());
        }
        else
        {
            Console.WriteLine(invoiceResult.ResultError.ErrorMessage);
            Alert.ShowError("Ha habido un error al guardar la factura.");
        }

    }
}
