using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using DistribuidoraGustavo.Interfaces.Shared;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ListResult<TransactionModel>> Get([FromQuery] int? clientId)
        {
            try
            {
                var transactions = await _transactionService.GetTransactions(clientId);
                return ListResult<TransactionModel>.Ok(transactions);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ListResult<TransactionModel>.Error(ex);
            }
        }

        [HttpPost("payment")]
        public async Task<DTOResult<TransactionModel>> PostPayment([FromBody] TransactionModel model)
        {
            try
            {
                var transaction = await _transactionService.CreatePayment(model);
                return DTOResult<TransactionModel>.Ok(transaction);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return DTOResult<TransactionModel>.Error(ex);
            }
        }

    }
}
