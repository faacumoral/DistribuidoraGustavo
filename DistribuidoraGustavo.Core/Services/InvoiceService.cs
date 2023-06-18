using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services
{
    public class InvoiceService : BaseService, IInvoiceService
    {
        private readonly DistribuidoraGustavoContext _context;

        public InvoiceService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InvoiceModel>> GetAll()
        {
            var invoices = await _context.Invoices.Where(i => i.Active)
                .Include(i => i.Client)
                .Include(i => i.InvoicesProducts).ThenInclude(ip => ip.Product)
                .Include(i => i.PriceList)
            .ToListAsync();

            var invoicesModel = invoices.Select(i => i.ToModel()).ToList();
            return invoicesModel;
        }
    }
}
