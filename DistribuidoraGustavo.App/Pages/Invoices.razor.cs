using DistribuidoraGustavo.App.Http;
using DistribuidoraGustavo.App.Shared;
using DistribuidoraGustavo.App.Utils;
using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;

public class InvoicesBase : ComponentBase
{
    public Alerts Alert { get; set; }

    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    protected IList<InvoiceModel> Invoices { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var apiRequest = ApiRequest.BuildGet("Invoices");

        var invoicesResut = await ApiClient.Send<ListResult<InvoiceModel>>(apiRequest);

        if (invoicesResut.Success)
        {
            Invoices = invoicesResut.ResultOk;
        }

        await base.OnInitializedAsync();
    }


    protected void GoToAddInvoice()
    {
        NavigationManager.NavigateTo(Views.AddInvoice.ToString());
    }

}
