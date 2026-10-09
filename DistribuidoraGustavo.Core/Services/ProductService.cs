using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using DistribuidoraGustavo.Interfaces.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;

namespace DistribuidoraGustavo.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly DistribuidoraGustavoContext _context;
        private readonly IPriceListService _priceListService;

        public ProductService(DistribuidoraGustavoContext context, IPriceListService priceListService)
        {
            _context = context;
            _priceListService = priceListService;
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
                    QuantityPerBox = product.QuantityPerBox,
                    BasePrice = product.BasePrice,
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

        public async Task<DTOResult<ProductModel>> Upsert(ProductModel model)
        {
            var product = await _context.Products.Include(p => p.ProductsPriceLists).FirstOrDefaultAsync(p => p.ProductId == model.ProductId);

            if (string.IsNullOrEmpty(model.Name))
                return DTOResult<ProductModel>.Error($"El nombre es obligatorio");

            if (product == null)
            {
                if (string.IsNullOrEmpty(model.Code))
                    return DTOResult<ProductModel>.Error($"Código y nombre son campos obligatorios");

                product = await _context.Products.Include(p => p.ProductsPriceLists).FirstOrDefaultAsync(p => p.Code == model.Code);

                if (product != null && product.Active == true)
                    return DTOResult<ProductModel>.Error($"El código '{model.Code}' ya esta registrado");

                if (product == null)
                {
                    product = new Product { Code = model.Code };
                    _context.Products.Add(product);
                }

                // a deleted product with the same code comes back with the new data
                product.Active = true;
            }

            product.BasePrice = model.BasePrice;
            product.Name = model.Name;
            product.Description = model.Description;
            product.QuantityPerBox = model.QuantityPerBox;

            var pricesList = await _context.PriceLists.ToListAsync();
            _priceListService.CalculatePrices(product, pricesList);

            await _context.SaveChangesAsync();

            return DTOResult<ProductModel>.Ok(product.ToModel());
        }

        public async Task<BoolResult> Delete(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return BoolResult.Error("El producto no existe");

            // logical delete: old invoices keep pointing to the product
            product.Active = false;
            await _context.SaveChangesAsync();

            return BoolResult.Ok();
        }

        public async Task<ProductModel> GetById(int productId)
        { 
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            return product.ToModel();
        }
    }
}
