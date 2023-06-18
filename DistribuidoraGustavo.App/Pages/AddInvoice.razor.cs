using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;

public class AddInvoiceBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }

    protected IList<ProductInvoiceModel> ProductInvoices { get; set; } = new List<ProductInvoiceModel>();

    protected IList<ProductModel> Products { get; set; } = new List<ProductModel>();

    protected IList<ClientModel> Clients { get; set; } = new List<ClientModel>();

    protected IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    protected string ProductsFilter;

    protected override async Task OnInitializedAsync()
    {
        var clientsApiRequest = ApiRequest.BuildGet("Clients");
        var priceListsApiRequest = ApiRequest.BuildGet("PriceLists");
        var productsApiRequest = ApiRequest.BuildGet("Products");

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(clientsApiRequest);
        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(priceListsApiRequest);
        var productsResult = await ApiClient.Send<ListResult<ProductModel>>(productsApiRequest);

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk;

        if (productsResult.Success)
            Products = productsResult.ResultOk;

        await base.OnInitializedAsync();
    }


}
