using DistribuidoraGustavo.Core.Enums;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly DistribuidoraGustavoContext _context;

        public TransactionService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task DeleteInvoiceTransaction(int clientId, string invoiceNumber)
        { 
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.ClientId == clientId && t.Description == invoiceNumber);

            if (transaction is not null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateInvoiceTransaction(int clientId, string invoiceNumber, decimal newAmount)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.ClientId == clientId && t.Description == invoiceNumber);

            if (transaction is not null)
            {
                transaction.Amount = newAmount;
                _context.Transactions.Update(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TransactionModel> CreateInvoiceTransaction(int clientId, decimal invoiceAmount, string invoiceNumber)
        {
            var transaction = new Transaction
            {
                Amount = invoiceAmount,
                ClientId = clientId,
                Date = DateTime.UtcNow,
                Description = invoiceNumber,
                Type = eTransactionType.Invoice.ToString()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction.ToModel();
        }

        public async Task<TransactionModel> CreatePayment(TransactionModel model)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == model.Client.ClientId);
            if (client == null)
            {
                throw new NullReferenceException("El cliente no existe");
            }

            client.ActualBalance -= model.Amount;

            var transaction = new Transaction
            {
                Amount = -model.Amount,
                ClientId = model.Client.ClientId,
                Date = DateTime.UtcNow,
                Description = "Pago",
                Type = eTransactionType.Payment.ToString()
            };

            _context.Transactions.Add(transaction);
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
            return transaction.ToModel();
        }

        public async Task<List<TransactionModel>> GetTransactions(int? clientId)
        {
            var transactionsQuery = _context.Transactions.Include(t => t.Client).AsQueryable();

            if (clientId.HasValue && clientId > 0)
            {
                transactionsQuery = transactionsQuery.Where(t => t.ClientId == clientId.Value);
            }

            var transaction = await transactionsQuery.OrderByDescending( t => t.Date).Take(200).ToListAsync();

            return transaction.Select(CastEfToModel.ToModel).ToList();
        }
        
    }
}
