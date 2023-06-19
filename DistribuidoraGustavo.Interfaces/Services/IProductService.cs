using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IProductService : IBaseService
{
    public Task<IList<ProductModel>> GetAll(string filter = "", int? priceListId = 0);
}
