using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;

namespace DistribuidoraGustavo.Interfaces.Services
{
    public interface IInvoiceService : IBaseService
    {
        Task<IReadOnlyList<InvoiceModel>> GetAll();
        Task<DTOResult<InvoiceModel>> InsertInvoice(InvoiceModel invoice);
    }
}
