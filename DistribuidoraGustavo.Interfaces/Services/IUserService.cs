using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Responses;
using DistribuidoraGustavo.Interfaces.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IUserService : IBaseService
{
    public Task<DTOResult<LoginResponse>> TryLogin(LoginRequest request);
}
