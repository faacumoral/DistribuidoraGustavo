using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IClientService : IBaseService
{
    Task<IList<ClientModel>> GetAll();
    Task<ClientModel> GetById(int clientId);
}
