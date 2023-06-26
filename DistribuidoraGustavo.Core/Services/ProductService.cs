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

        private async Task<IList<ProductModel>> SearchProducts(string filter, int quantity = 100)
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

            var products = await productsDb.Take(quantity).ToListAsync();

            var productsModel = products.Select(CastEfToModel.ToModel).ToList();

            return productsModel;
        }


        public async Task<IList<ProductModel>> GetAll(string filter = "", int? priceListId = 0)
        {
            var productsModel = await SearchProducts(filter, 100);

            if (priceListId != null && productsModel.Count > 0)
            {
                var priceList = await _context.PriceLists.FirstOrDefaultAsync(pl => pl.PriceListId == priceListId);

                if (priceList != null)
                {
                    var productsId = productsModel.Select(pm => pm.ProductId);
                    var prices = await _context.ProductsPriceLists
                        .Where(ppl => ppl.PriceListId == priceListId)
                        .Where(ppl => productsId.Contains(ppl.ProductId)).ToListAsync();

                    foreach (var product in productsModel)
                    {
                        product.UnitPrice = prices.FirstOrDefault(p => p.ProductId == product.ProductId)?.Price ?? 0;
                    }
                }
            }

            return productsModel;
        }

        public async Task<IList<PricedProductModel>> GetWithPrices(string filter, int? priceListId)
        {
            var pricedProducts = new List<PricedProductModel>();
            var priceListsId = new List<int>();

            var productsModel = await SearchProducts(filter, 300);

            if (priceListId == null || priceListId == 0)
                priceListsId = await _context.PriceLists.Select(pl => pl.PriceListId).ToListAsync();
            else
                priceListsId.Add(priceListId.Value);

            var productsIds = productsModel.Select(pm => pm.ProductId).ToList();

            // get all prices for the products
            var prices = await _context.ProductsPriceLists
                    .Where(ppl => priceListsId.Contains(ppl.PriceListId) && productsIds.Contains(ppl.ProductId))
                    .Include(ppl => ppl.PriceList)
                    .ToListAsync();

            foreach (var product in productsModel)
            {
                var pricedProductModel = new PricedProductModel
                {
                    ProductId = product.ProductId,
                    Code = product.Code,
                    Description = product.Description,
                    Name = product.Name,
                    Prices = prices.Where(p => p.ProductId == product.ProductId).Select(
                        pr => new ProductPriceModel
                        { 
                            Price = pr.Price,
                            PriceListModel = pr.PriceList.ToModel()
                        }).ToList()
                    };

                pricedProducts.Add(pricedProductModel);
            }

            return pricedProducts;

        }
    }
}
