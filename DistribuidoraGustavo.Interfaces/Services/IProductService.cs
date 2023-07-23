using DistribuidoraGustavo.Interfaces.Models;
using FMCW.Common.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IProductService : IBaseService
{
    Task<IList<ProductModel>> GetAll(string filter = "", int? priceListId = 0);
    Task<IList<PricedProductModel>> GetWithPrices(string filter, int? priceListId);
    Task<DTOResult<ProductModel>> Upsert(ProductModel model);
    Task<ProductModel> GetById(int productId);
}
