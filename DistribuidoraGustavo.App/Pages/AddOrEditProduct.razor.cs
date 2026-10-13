using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;
using DistribuidoraGustavo.Interfaces.Shared;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;


public class AddOrEditProductState
{
    public bool Processing { get; set; } = false;
    public bool ConfirmingDelete { get; set; } = false;

    // name as it is saved: the title does not change while the name field is being edited
    public string SavedName { get; set; }

    // what is typed in the numeric inputs, parsed on every change
    public string PriceText { get; set; } = "";
    public string QuantityText { get; set; } = "";

    public string CodeError { get; set; }
    public string NameError { get; set; }
    public string PriceError { get; set; }
    public string QuantityError { get; set; }
}


public class AddOrEditProductBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    [Parameter] public int ProductId { get; set; }

    public Alerts Alert { get; set; }

    public ProductModel Product { get; set; } = new ProductModel();

    public IList<PriceListModel> PriceLists { get; set; } = new List<PriceListModel>();

    public IList<FamilyModel> Families { get; set; } = new List<FamilyModel>();

    public AddOrEditProductState State { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        State.Processing = true;

        var priceListResult = await ApiClient.Send<ListResult<PriceListModel>>(ApiRequest.BuildGet("PriceLists"));

        if (priceListResult.Success)
            PriceLists = priceListResult.ResultOk.OrderBy(pl => pl.PriceListId).ToList();

        var familiesResult = await ApiClient.Send<ListResult<FamilyModel>>(ApiRequest.BuildGet("Families"));

        if (familiesResult.Success)
            Families = familiesResult.ResultOk;

        if (ProductId != 0)
        {
            await LoadProduct();
        }

        State.Processing = false;

        await base.OnInitializedAsync();
    }

    protected async Task LoadProduct()
    {
        Product.ProductId = ProductId;

        var request = ApiRequest.BuildGet($"Products/{ProductId}");
        var productResult = await ApiClient.Send<DTOResult<ProductModel>>(request);

        if (productResult.Success)
        {
            Product = productResult.ResultOk;
            State.SavedName = Product.Name;
            State.PriceText = Product.BasePrice.ToEditable();
            State.QuantityText = Product.QuantityPerBox == 0 ? "" : Product.QuantityPerBox.ToString();
        }
        else
        {
            Alert.ShowError(productResult.ResultError.FriendlyErrorMessage, false);
        }
    }

    protected void FamilyChanged(ChangeEventArgs e)
    {
        var previous = Families.FirstOrDefault(f => f.FamilyId == Product.FamilyId);
        var selected = int.TryParse(e.Value?.ToString(), out int familyId)
            ? Families.FirstOrDefault(f => f.FamilyId == familyId)
            : null;

        Product.FamilyId = selected?.FamilyId;
        Product.FamilyName = selected?.Name;

        // the code is read-only when editing; on a new product, only the previous suffix is swapped
        if (ProductId != 0) return;

        var code = Product.Code ?? "";
        var oldSuffix = previous?.CodeSuffix ?? "";
        var newSuffix = selected?.CodeSuffix ?? "";

        if (oldSuffix != "" && code.StartsWith(oldSuffix, StringComparison.OrdinalIgnoreCase))
            code = code.Substring(oldSuffix.Length);

        if (newSuffix != "" && !code.StartsWith(newSuffix, StringComparison.OrdinalIgnoreCase))
            code = newSuffix + code;

        Product.Code = code;
        State.CodeError = null;
        HideAlertIfFixed();
    }

    protected void CodeChanged(ChangeEventArgs e)
    {
        Product.Code = e.Value?.ToString().Trim();
        State.CodeError = null;
        HideAlertIfFixed();
    }

    protected void NameChanged(ChangeEventArgs e)
    {
        Product.Name = e.Value?.ToString();
        State.NameError = null;
        HideAlertIfFixed();
    }

    protected void PriceChanged(ChangeEventArgs e)
    {
        State.PriceText = e.Value?.ToString();
        State.PriceError = null;
        HideAlertIfFixed();
    }

    protected void QuantityChanged(ChangeEventArgs e)
    {
        State.QuantityText = e.Value?.ToString();
        State.QuantityError = null;
        HideAlertIfFixed();
    }

    protected string PreviewPrice(PriceListModel priceList)
    {
        if (!Numbers.TryParseAmount(State.PriceText, out decimal basePrice))
            return "-";

        return PriceCalculator.Calculate(basePrice, priceList.Percent ?? 0).ToMoney();
    }

    private bool HasFieldErrors => State.CodeError != null || State.NameError != null
        || State.PriceError != null || State.QuantityError != null;

    private void HideAlertIfFixed()
    {
        if (!HasFieldErrors) Alert.Hide();
    }

    private bool Validate()
    {
        if (ProductId == 0 && string.IsNullOrWhiteSpace(Product.Code))
            State.CodeError = "Escribí el código del producto.";
        else if (ProductId == 0 && Families.Any(f => string.Equals(f.CodeSuffix, Product.Code, StringComparison.OrdinalIgnoreCase)))
            State.CodeError = "Completá el código: agregale el número después del subfijo.";

        if (string.IsNullOrWhiteSpace(Product.Name))
            State.NameError = "Escribí el nombre del producto.";

        if (Numbers.TryParseAmount(State.PriceText, out decimal basePrice))
            Product.BasePrice = basePrice;
        else
            State.PriceError = "Escribí el precio solo con números. Por ejemplo: 1500 o 1500,50";

        if (string.IsNullOrWhiteSpace(State.QuantityText))
            Product.QuantityPerBox = 0;
        else if (int.TryParse(State.QuantityText.Trim(), out int quantity) && quantity >= 0)
            Product.QuantityPerBox = quantity;
        else
            State.QuantityError = "Escribí la cantidad solo con números. Por ejemplo: 12";

        return !HasFieldErrors;
    }

    protected async Task SaveProduct()
    {
        if (!Validate())
        {
            Alert.ShowError("No se pudo guardar. Revisá los campos marcados en rojo.", false);
            return;
        }

        Product.Name = Product.Name.Trim();
        // there is no description field in this form: it follows the name, as the Excel import does
        Product.Description = Product.Name;

        var request = ApiRequest.BuildPost("Products", Product);
        State.Processing = true;
        var productResult = await ApiClient.Send<DTOResult<ProductModel>>(request);
        State.Processing = false;

        if (productResult.Success)
        {
            NavigationManager.NavigateTo($"{Views.Products}?done={ProductsBase.SAVED_NOTICE}");
        }
        else
        {
            Alert.ShowError(productResult.ResultError.FriendlyErrorMessage, false);
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo(Views.Products.ToString());
    }

    protected void AskDelete()
    {
        State.ConfirmingDelete = true;
    }

    protected void CancelDelete()
    {
        State.ConfirmingDelete = false;
    }

    protected async Task DeleteProduct()
    {
        State.Processing = true;
        var result = await ApiClient.Send<BoolResult>(ApiRequest.BuildDelete($"Products/{ProductId}"));
        State.Processing = false;

        if (result.Success)
        {
            NavigationManager.NavigateTo($"{Views.Products}?done={ProductsBase.DELETED_NOTICE}");
        }
        else
        {
            State.ConfirmingDelete = false;
            Alert.ShowError(result.ResultError.FriendlyErrorMessage, false);
        }
    }

}
