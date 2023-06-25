using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services
{
    public interface IPdfService
    {
        public Task<byte[]> GenerateInvoicePdf(InvoiceModel invoice);
    }
}
