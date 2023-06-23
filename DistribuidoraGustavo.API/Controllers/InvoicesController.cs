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
        public async Task<ListResult<InvoiceModel>> Get([FromQuery] int? ClientID = 0)
        {
            try
            {
                var invoices = await _invoiceService.GetAll(ClientID ?? 0);
                return ListResult<InvoiceModel>.Ok(invoices.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<InvoiceModel>.Error(ex);
            }
        }

        [HttpGet("{invoiceId:int}")]
        public async Task<DTOResult<InvoiceModel>> Get([FromRoute] int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetById(invoiceId);
                return DTOResult<InvoiceModel>.Ok(invoice);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<InvoiceModel>.Error(ex);
            }
        }

        [HttpPost]
        public async Task<DTOResult<InvoiceModel>> Post([FromBody] InvoiceModel invoiceModel)
        {
            try
            {
                var invoice = await _invoiceService.UpsertInvoice(invoiceModel);
                return invoice;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<InvoiceModel>.Error(ex);
            }
        }

    }
}
