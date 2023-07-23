using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IPriceListService : IBaseService
{
    public Task<IList<PriceListModel>> GetAll();

    Task<IList<string>> ProcessFile(string fileName, Stream stream);

    void CalculatePrices(Product product, List<PriceList> pricesLists);
}
