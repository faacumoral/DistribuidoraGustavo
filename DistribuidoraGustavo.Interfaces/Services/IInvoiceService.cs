using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;

namespace DistribuidoraGustavo.Interfaces.Services
{
    public interface IInvoiceService : IBaseService
    {
        Task<IReadOnlyList<InvoiceModel>> GetAll(int clientId = 0);
        Task<DTOResult<InvoiceModel>> UpsertInvoice(InvoiceModel invoice);
        Task<InvoiceModel> GetById(int invoiceId);
        Task<string> GetDownloadToken(int userId, int invoiceId);
        Task<DTOResult<DownloadModel>> DownloadInvoice(string token);
        Task<BoolResult> DeleteInvoice(int invoiceId);
    }
}
