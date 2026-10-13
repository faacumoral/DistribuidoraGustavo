using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;


public class AddOrEditFamilyState
{
    public bool Processing { get; set; } = false;
    public bool ConfirmingDelete { get; set; } = false;

    // name as it is saved: the title does not change while the name field is being edited
    public string SavedName { get; set; }

    public string NameError { get; set; }
    public string SuffixError { get; set; }
}


public class AddOrEditFamilyBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    [Parameter] public int FamilyId { get; set; }

    public Alerts Alert { get; set; }

    public FamilyModel Family { get; set; } = new FamilyModel();

    public AddOrEditFamilyState State { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (FamilyId != 0)
        {
            State.Processing = true;
            await LoadFamily();
            State.Processing = false;
        }

        await base.OnInitializedAsync();
    }

    protected async Task LoadFamily()
    {
        var result = await ApiClient.Send<DTOResult<FamilyModel>>(ApiRequest.BuildGet($"Families/{FamilyId}"));

        if (result.Success)
        {
            Family = result.ResultOk;
            State.SavedName = Family.Name;
        }
        else
        {
            Alert.ShowError(result.ResultError.FriendlyErrorMessage, false);
        }
    }

    protected void NameChanged(ChangeEventArgs e)
    {
        Family.Name = e.Value?.ToString();
        State.NameError = null;
        HideAlertIfFixed();
    }

    protected void SuffixChanged(ChangeEventArgs e)
    {
        Family.CodeSuffix = e.Value?.ToString().Trim().ToUpper();
        State.SuffixError = null;
        HideAlertIfFixed();
    }

    private bool HasFieldErrors => State.NameError != null || State.SuffixError != null;

    private void HideAlertIfFixed()
    {
        if (!HasFieldErrors) Alert.Hide();
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Family.Name))
            State.NameError = "Escribí el nombre de la familia.";

        if (string.IsNullOrWhiteSpace(Family.CodeSuffix))
            State.SuffixError = "Escribí el subfijo. Por ejemplo: GAS";

        return !HasFieldErrors;
    }

    protected async Task SaveFamily()
    {
        if (!Validate())
        {
            Alert.ShowError("No se pudo guardar. Revisá los campos marcados en rojo.", false);
            return;
        }

        Family.Name = Family.Name.Trim();

        State.Processing = true;
        var result = await ApiClient.Send<DTOResult<FamilyModel>>(ApiRequest.BuildPost("Families", Family));
        State.Processing = false;

        if (result.Success)
            NavigationManager.NavigateTo($"{Views.Families}?done={FamiliesBase.SAVED_NOTICE}");
        else
            Alert.ShowError(result.ResultError.FriendlyErrorMessage, false);
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo(Views.Families.ToString());
    }

    protected void AskDelete()
    {
        State.ConfirmingDelete = true;
    }

    protected void CancelDelete()
    {
        State.ConfirmingDelete = false;
    }

    protected async Task DeleteFamily()
    {
        State.Processing = true;
        var result = await ApiClient.Send<BoolResult>(ApiRequest.BuildDelete($"Families/{FamilyId}"));
        State.Processing = false;

        if (result.Success)
        {
            NavigationManager.NavigateTo($"{Views.Families}?done={FamiliesBase.DELETED_NOTICE}");
        }
        else
        {
            State.ConfirmingDelete = false;
            Alert.ShowError(result.ResultError.FriendlyErrorMessage, false);
        }
    }

}
