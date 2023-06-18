using DistribuidoraGustavo.Core.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class InvoicesController : BaseController
    {

        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<ListResult<InvoiceModel>> Get()
        {
            try
            {
                var invoices = await _invoiceService.GetAll();
                return ListResult<InvoiceModel>.Ok(invoices.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<InvoiceModel>.Error(ex);
            }

        }

    }
}
