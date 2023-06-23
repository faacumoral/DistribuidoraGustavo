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

        public async Task<IReadOnlyList<InvoiceModel>> GetAll(int clientId = 0)
        {
            var invoices = await _context.Invoices
                .Where(i => i.Active)
                .Where(i => clientId == 0 || i.ClientId == clientId)
                .Include(i => i.Client)
                .Include(i => i.InvoicesProducts).ThenInclude(ip => ip.Product)
                .Include(i => i.PriceList)
                .Take(50)
                .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

            var invoicesModel = invoices.Select(i => i.ToModel()).ToList();
            return invoicesModel;
        }

        public async Task<InvoiceModel> GetById(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include(i => i.Client)
                .Include(i => i.InvoicesProducts).ThenInclude(ip => ip.Product)
                .Include(i => i.PriceList)
            .FirstOrDefaultAsync();

            if (invoice == null) throw new NullReferenceException();

            return invoice.ToModel();
        }

        public async Task<DTOResult<InvoiceModel>> UpsertInvoice(InvoiceModel invoiceModel)
        {
            Invoice? invoice;

            if (invoiceModel.Client == null || invoiceModel.Client.ClientId == 0)
                return DTOResult<InvoiceModel>.Error("La factura debe estar asociada a un cliente");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == invoiceModel.Client.ClientId);

            if (client == null)
                return DTOResult<InvoiceModel>.Error("Cliente invalido");

            if (invoiceModel.Products.Count == 0)
                return DTOResult<InvoiceModel>.Error("Seleccione al menos un producto");

            if (invoiceModel.InvoiceId == 0)
            {
                var invoiceNumbers = await _context.Invoices
                .Where(i => i.ClientId == client.ClientId).Select(i => i.InvoiceNumber).ToListAsync();

                var nextInvoiceNumber = 1;
                if (invoiceNumbers.Any())
                {
                    nextInvoiceNumber = invoiceNumbers
                    .Max(i => int.Parse(i.Replace(client.InvoicePrefix + "-", ""))) + 1;
                }

                invoice = new Invoice
                {
                    Active = true,
                    ClientId = invoiceModel.Client.ClientId,
                    CreatedDate = DateTime.UtcNow,
                    Description = invoiceModel.Description,
                    PriceListId = invoiceModel.PriceList.PriceListId,
                    InvoiceNumber = $"{client.InvoicePrefix}-{nextInvoiceNumber}",
                    InvoicesProducts = invoiceModel.Products.Select(p => new InvoicesProduct
                    {
                        Amount = p.Amount,
                        ProductId = p.ProductId,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    }).ToList()
                };

                _context.Invoices.Add(invoice);
            }
            else
            {
                invoice = await _context.Invoices.Include(i => i.InvoicesProducts)
                                                 .FirstOrDefaultAsync(i => i.InvoiceId == invoiceModel.InvoiceId);

                if (invoice == null)
                    return DTOResult<InvoiceModel>.Error("Factura inválida");

                invoice.Description = invoiceModel.Description;

                _context.InvoicesProducts.RemoveRange(invoice.InvoicesProducts);

                foreach (var product in invoiceModel.Products)
                {
                    var invoiceProduct = invoice.InvoicesProducts.FirstOrDefault(ip => ip.ProductId == product.ProductId);

                    if (invoiceProduct == null)
                    {
                        _context.InvoicesProducts.Add(new InvoicesProduct
                        {
                            Amount = product.Amount,
                            ProductId = product.ProductId,
                            Quantity = product.Quantity,
                            UnitPrice = product.UnitPrice,
                            InvoiceId = invoiceModel.InvoiceId
                        });
                    }
                    else
                    {
                        invoiceProduct.Amount = product.Amount;
                        invoiceProduct.Quantity = product.Quantity;
                        invoiceProduct.UnitPrice = product.UnitPrice;
                        _context.InvoicesProducts.Update(invoiceProduct);
                    }
                }
            }

            await _context.SaveChangesAsync();
            var insertedUpdatedInvoice = CastEfToModel.ToModel(invoice);
            return insertedUpdatedInvoice;
        }
    }
}
