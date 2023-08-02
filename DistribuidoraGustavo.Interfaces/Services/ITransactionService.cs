using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services
{
    public interface ITransactionService
    {
        Task<TransactionModel> CreatePayment(TransactionModel model);
        Task<TransactionModel> CreateInvoiceTransaction(int clientId, decimal invoiceAmount, string invoiceNumber);
        Task<List<TransactionModel>> GetTransactions(int? clientId);
        Task DeleteInvoiceTransaction(int clientId, string invoiceNumber);
        Task UpdateInvoiceTransaction(int clientId, string invoiceNumber, decimal newAmount);
    }
}
