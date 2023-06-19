using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly DistribuidoraGustavoContext _context;

        public ProductService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task<IList<ProductModel>> GetAll(string filter = "", int? priceListId = 0)
        {
            var productsDb = _context.Products.Where(p => p.Active == true);

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                productsDb = productsDb
                .Where(p => 
                   p.Name.ToLower().Contains(filter)
                || p.Code.ToLower().Contains(filter)
                || (p.Description != null && p.Description!.ToLower().Contains(filter))
                );
            }

            var products = await productsDb.Take(100).ToListAsync();

            return products.Select(CastEfToModel.ToModel).ToList();
        }
    }
}
