using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DistribuidoraGustavo.App.Pages;

public class InvoicesState
{
    public int SelectedClientID { get; set; } = 0;
    public bool Searching { get; set; } = false;
    public bool Processing { get; set; } = false;
}

public class InvoicesBase : ComponentBase
{
    public Alerts Alert { get; set; }

    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JSRuntime { get; set; }
    [Inject] IConfiguration Configuration { get; set; }

    protected InvoicesState State { get; set; } = new();

    protected IList<ClientModel> Clients { get; set; } = new List<ClientModel>();

    protected IList<InvoiceModel> Invoices { get; set; }


    protected override async Task OnInitializedAsync()
    {
        State.Searching = true;

        var clientsResult = await ApiClient.Send<ListResult<ClientModel>>(ApiRequest.BuildGet("Clients"));
        var invoicesResut = await ApiClient.Send<ListResult<InvoiceModel>>(ApiRequest.BuildGet("Invoices"));

        if (clientsResult.Success)
            Clients = clientsResult.ResultOk;

        if (invoicesResut.Success)
            Invoices = invoicesResut.ResultOk;

        State.Searching = false;

        await base.OnInitializedAsync();
    }

    protected async Task ClientChange(int clientId)
    {
        State.SelectedClientID = clientId;
        await SearchInvoicesForClient();
    }

    protected async Task SearchInvoicesForClient()
    {
        State.Searching = true;

        var invoicesResut = await ApiClient.Send<ListResult<InvoiceModel>>(ApiRequest.BuildGet($"Invoices?ClientId={State.SelectedClientID}"));

        if (invoicesResut.Success)
            Invoices = invoicesResut.ResultOk;

        State.Searching = false;
    }

    protected void EditInvoice(InvoiceModel invoice)
    {
        NavigationManager.NavigateTo(Views.AddOrEditInvoice.ToString() + $"/{invoice.InvoiceId}");
    }

    protected async Task DownloadInvoice(InvoiceModel invoice)
    {
        State.Processing = true;
        var urlToken = $"Invoices/{invoice.InvoiceId}/downloadtoken";

        var tokenResult = await ApiClient.Send<StringResult>(ApiRequest.BuildGet(urlToken));

        if (!tokenResult.Success)
        {
            Alert.ShowError("Ha habido un error al descargar el archivo");
            Console.WriteLine(tokenResult.ResultError.FullException);
        }

        var urlDownload = $"{Configuration["ApiUrl"]}invoices/download?token={tokenResult.ResultOk}";
        await JSRuntime.InvokeVoidAsync("eval", $"let _discard_ = open(`{urlDownload}`, `_blank`)");
        State.Processing = false;
    }

    protected async Task DeleteInvoice(InvoiceModel invoice)
    {
        State.Processing = true;
        var url = $"Invoices/{invoice.InvoiceId}";

        var result = await ApiClient.Send<BoolResult>(ApiRequest.BuildDelete(url));
        State.Processing = false;

        if (!result.Success)
        {
            Alert.ShowError("Ha habido un error al eliminar la factura");
            Console.WriteLine(result.ResultError.FullException);
        }
        else
        {
            await SearchInvoicesForClient();
        }
    }

    
}
