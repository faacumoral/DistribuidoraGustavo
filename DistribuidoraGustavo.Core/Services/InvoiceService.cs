using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
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

        public async Task<DTOResult<InvoiceModel>> InsertInvoice(InvoiceModel invoiceModel)
        {
            if (invoiceModel.Client == null || invoiceModel.Client.ClientId == 0)
                return DTOResult<InvoiceModel>.Error("La factura debe estar asociada a un cliente");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == invoiceModel.Client.ClientId);

            if (client == null)
                return DTOResult<InvoiceModel>.Error("Cliente invalido");
            
            if (invoiceModel.Products.Count == 0)
                return DTOResult<InvoiceModel>.Error("Seleccione al menos un producto");

            var invoiceNumbers = await _context.Invoices
                .Where(i => i.ClientId == client.ClientId).Select(i => i.InvoiceNumber).ToListAsync();

            var nextInvoiceNumber = invoiceNumbers
                .Max(i => int.Parse(i.Replace(client.InvoicePrefix + "-", ""))) + 1;

            var invoice = new Invoice
            {
                Active = true,
                ClientId = invoiceModel.Client.ClientId,
                CreatedDate = DateTime.UtcNow,
                Description = invoiceModel.Description,
                PriceListId = invoiceModel.PriceList.PriceListId,
                InvoiceNumber = $"{client.InvoicePrefix}-{nextInvoiceNumber}",
                InvoicesProducts = invoiceModel.Products.Select( p => new InvoicesProduct { 
                    Amount = p.Amount,
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                }).ToList()
            };

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();
            var insertedInvoice = CastEfToModel.ToModel(invoice);
            return insertedInvoice;
        }
    }
}
