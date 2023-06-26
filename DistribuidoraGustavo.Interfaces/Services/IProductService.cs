using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IProductService : IBaseService
{
    Task<IList<ProductModel>> GetAll(string filter = "", int? priceListId = 0);
    Task<IList<PricedProductModel>> GetWithPrices(string filter, int? priceListId);
}
