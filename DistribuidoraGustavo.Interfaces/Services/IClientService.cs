using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IClientService : IBaseService
{
    Task<IList<ClientModel>> GetAll();
    Task<ClientModel> GetById(int clientId);
    Task<DTOResult<ClientModel>> UpdateClient(ClientModel clientModel);
    Task<DTOResult<ClientModel>> InsertClient(ClientModel clientModel);
}
