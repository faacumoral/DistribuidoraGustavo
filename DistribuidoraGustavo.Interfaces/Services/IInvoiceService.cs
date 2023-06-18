using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services
{
    public interface IInvoiceService : IBaseService
    {
        Task<IReadOnlyList<InvoiceModel>> GetAll();

    }
}
