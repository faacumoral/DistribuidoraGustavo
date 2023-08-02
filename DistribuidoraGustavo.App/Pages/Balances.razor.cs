using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;


public class BalancesState
{
    public bool Processing { get; set; } = false;
    public int Amount { get; set; }
    public ClientModel Client { get; set; } 
}

public class BalancesBase : ComponentBase
{
    #region DI
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    #endregion

    public BalancesState State { get; set; } = new();

    public Alerts Alert { get; set; }
    public IList<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();
    public IList<ClientModel> Clients { get; set; } = new List<ClientModel>();

    public HiddenFileInput FileInput { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SearchTransactions();

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(ApiRequest.BuildGet("Clients"));

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        await base.OnInitializedAsync();
    }

    protected async void SearchTransactions()
    {
        State.Processing = true;

        var transactionsResult = await ApiClient.Send<ListResult<TransactionModel>>(ApiRequest.BuildGet($"Transactions?ClientID={State.Client?.ClientId}"));

        if (transactionsResult.Success)
            Transactions = transactionsResult.ResultOk;

        State.Processing = false;
        StateHasChanged();
    }

    protected void ClientChange(int clientId)
    {   
        State.Client = Clients.FirstOrDefault(c => c.ClientId == clientId);
        SearchTransactions();
    }

    protected async Task SaveNewTransaction()
    {
        if (State.Amount <= 0)
        {
            Alert.ShowError("El importe debe ser positivo");
            return;
        }

        if (State.Client is null || State.Client.ClientId <= 0)
        {
            Alert.ShowError("Seleccione un cliente");
            return;
        }

        var request = ApiRequest.BuildPost("Transactions/payment", new TransactionModel
        {
            Amount = State.Amount,
            Client = State.Client
        }); 

        State.Processing = true;
        var transactionResult = await ApiClient.Send<DTOResult<TransactionModel>>(request);
        State.Processing = false;

        if (transactionResult.Success)
        {
            Transactions.Insert(0, transactionResult.ResultOk);
            State.Client.ActualBalance -= State.Amount;
            State.Amount = 0;
        }
        else
            Alert.ShowError(transactionResult.ResultError.FriendlyErrorMessage);

    }

}
