using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IClientService : IBaseService
{
    public Task<IList<ClientModel>> GetAll();
}
