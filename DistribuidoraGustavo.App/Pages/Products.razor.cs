using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;


public class ProductsState
{
    public bool Processing { get; set; } = false;
    public string Filter { get; set; } = "";
    public string Notice { get; set; }
}


public class ProductsBase : ComponentBase
{
    public const string SAVED_NOTICE = "saved";
    public const string DELETED_NOTICE = "deleted";

    #region DI
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    #endregion

    public Alerts Alert { get; set; }
    public IList<PricedProductModel> Products { get; set; } = new List<PricedProductModel>();
    public IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    public SearchInput SearchInput { get; set; }
    public ProductsState State { get; set; } = new ProductsState();

    protected override async Task OnInitializedAsync()
    {
        // set by AddOrEditProduct when it comes back here
        var query = new Uri(NavigationManager.Uri).Query;
        if (query.Contains($"done={SAVED_NOTICE}"))
            State.Notice = "Producto guardado";
        else if (query.Contains($"done={DELETED_NOTICE}"))
            State.Notice = "Producto dado de baja";

        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(ApiRequest.BuildGet("PriceLists"));

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk.OrderBy(pl => pl.PriceListId).ToList();

        await SearchProducts();

        await base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(State.Notice))
            Alert.ShowSuccess(State.Notice);
    }

    protected async Task SearchProducts()
    {
        State.Processing = true;
        StateHasChanged();

        var url = $"products/prices?filter={Uri.EscapeDataString(State.Filter ?? "")}&priceListId=0";

        var productsResult = await ApiClient.Send<ListResult<PricedProductModel>>(ApiRequest.BuildGet(url));

        if (productsResult.Success)
            Products = productsResult.ResultOk;
        else
            Alert.ShowError(productsResult.ResultError.FriendlyErrorMessage, false);

        State.Processing = false;

        await SearchInput.Focus();

        StateHasChanged();
    }

    protected string PriceIn(PricedProductModel product, PriceListModel priceList)
    {
        var price = product.Prices?.FirstOrDefault(p => p.PriceListModel?.PriceListId == priceList.PriceListId);
        return price == null ? "-" : price.Price.ToMoney();
    }

    protected void AddProduct()
    {
        NavigationManager.NavigateTo(Views.AddOrEditProduct.ToString());
    }

    protected void UpdateProduct(ProductModel model)
    {
        NavigationManager.NavigateTo(Views.AddOrEditProduct.ToString() + $"/{model.ProductId}");
    }

}
