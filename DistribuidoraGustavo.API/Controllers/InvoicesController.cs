using DistribuidoraGustavo.Core.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Authorization;
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

        [HttpDelete("{invoiceId:int}")]
        public async Task<BoolResult> DeleteInvoice([FromRoute] int invoiceId)
        {
            try
            {
                var result = await _invoiceService.DeleteInvoice(invoiceId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return BoolResult.Error(ex);
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


        [HttpGet("{invoiceID:int}/downloadtoken")]
        public async Task<StringResult> GetDownloadToken([FromRoute] int invoiceID)
        {
            try
            {
                var token = await _invoiceService.GetDownloadToken(UserId, invoiceID);
                return StringResult.Ok(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StringResult.Error(ex);
            }
        }

        [AllowAnonymous]
        [HttpGet("download")]
        public async Task<IActionResult> DownloadInvoice([FromQuery] string token)
        {
            try
            {
                var fileResult = await _invoiceService.DownloadInvoice(token);
                if (fileResult.Success)
                {
                    var file = fileResult.ResultOk;
                    return File(
                        file.Content,
                        file.ContentType,
                        file.Name);
                }

                return StatusCode(500, fileResult.ResultError.FriendlyErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return StatusCode(500);
            }
        }

    }
}
