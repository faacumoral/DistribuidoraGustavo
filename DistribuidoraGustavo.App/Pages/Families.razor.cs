using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;


public class FamiliesState
{
    public bool Processing { get; set; } = false;
    public string Filter { get; set; } = "";
    public string Notice { get; set; }
}


public class FamiliesBase : ComponentBase
{
    public const string SAVED_NOTICE = "saved";
    public const string DELETED_NOTICE = "deleted";

    #region DI
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    #endregion

    public Alerts Alert { get; set; }
    public IList<FamilyModel> Families { get; set; } = new List<FamilyModel>();

    public SearchInput SearchInput { get; set; }
    public FamiliesState State { get; set; } = new FamiliesState();

    protected override async Task OnInitializedAsync()
    {
        // set by AddOrEditFamily when it comes back here
        var query = new Uri(NavigationManager.Uri).Query;
        if (query.Contains($"done={SAVED_NOTICE}"))
            State.Notice = "Familia guardada";
        else if (query.Contains($"done={DELETED_NOTICE}"))
            State.Notice = "Familia eliminada";

        await SearchFamilies();

        await base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(State.Notice))
            Alert.ShowSuccess(State.Notice);
    }

    protected async Task SearchFamilies()
    {
        State.Processing = true;
        StateHasChanged();

        var url = $"families?filter={Uri.EscapeDataString(State.Filter ?? "")}";

        var result = await ApiClient.Send<ListResult<FamilyModel>>(ApiRequest.BuildGet(url));

        if (result.Success)
            Families = result.ResultOk;
        else
            Alert.ShowError(result.ResultError.FriendlyErrorMessage, false);

        State.Processing = false;

        await SearchInput.Focus();

        StateHasChanged();
    }

    protected void AddFamily()
    {
        NavigationManager.NavigateTo(Views.AddOrEditFamily.ToString());
    }

    protected void UpdateFamily(FamilyModel model)
    {
        NavigationManager.NavigateTo(Views.AddOrEditFamily.ToString() + $"/{model.FamilyId}");
    }

}
