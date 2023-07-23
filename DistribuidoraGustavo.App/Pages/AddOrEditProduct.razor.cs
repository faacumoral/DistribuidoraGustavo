using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;


public class AddOrEditProductBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }

    [Parameter] public int ProductId { get; set; }

    public Alerts Alert { get; set; }

    public ProductModel Product { get; set; } = new ProductModel();

    public bool Processing { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Processing = true;

        if (ProductId != 0)
        {
            await LoadProduct();
        }

        Processing = false;

        await base.OnInitializedAsync();
    }

    protected async Task LoadProduct()
    {
        Product.ProductId = ProductId;

        var request = ApiRequest.BuildGet($"Products/{ProductId}");
        Processing = true;
        var productResult = await ApiClient.Send<DTOResult<ProductModel>>(request);
        Processing = false;

        if (productResult.Success)
        {
            Product = productResult.ResultOk;
        }
        else
        {
            Alert.ShowError(productResult.ResultError.FriendlyErrorMessage);
        }
    }

    protected async Task SaveProduct()
    {
        var request = ApiRequest.BuildPost("Products", Product);
        Processing = true;
        var productResult = await ApiClient.Send<DTOResult<ProductModel>>(request);
        Processing = false;

        if (productResult.Success)
        {
            NavigationManager.NavigateTo(Views.Products.ToString());
        }
        else
        {
            Alert.ShowError(productResult.ResultError.FriendlyErrorMessage);
        }
    }

}
