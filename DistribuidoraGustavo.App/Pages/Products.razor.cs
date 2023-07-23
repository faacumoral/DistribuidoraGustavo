using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;


public class ProductsState
{
    public bool Processing { get; set; } = false;
    public string Filter { get; set; } = "";
}


public class ProductsBase : ComponentBase
{
    #region DI
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    #endregion

    public Alerts Alert { get; set; }
    public IList<ProductModel> Products { get; set; } = new List<ProductModel>();

    public SearchInput SearchInput { get; set; }
    public ProductsState State { get; set; } = new ProductsState();

    protected override async Task OnInitializedAsync()
    {
        await SearchProducts();

        await base.OnInitializedAsync();
    }

    protected async Task SearchProducts()
    {
        State.Processing = true;
        StateHasChanged();

        var url = $"products?filter={State.Filter}";

         var productsResult = await ApiClient.Send<ListResult<ProductModel>>(ApiRequest.BuildGet(url));

        if (productsResult.Success)
            Products = productsResult.ResultOk;
        else
            Alert.ShowError(productsResult.ResultError.FriendlyErrorMessage);

        State.Processing = false;

        await SearchInput.Focus();

        StateHasChanged();
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
