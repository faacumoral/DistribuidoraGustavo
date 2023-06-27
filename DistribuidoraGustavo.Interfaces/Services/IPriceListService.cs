using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IPriceListService : IBaseService
{
    public Task<IList<PriceListModel>> GetAll();

    Task<IList<string>> ProcessFile(Stream stream);
}
