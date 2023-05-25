using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Requests;
using FMCW.Common.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IUserService : IBaseService
{
    public Task<DTOResult<UserModel>> TryLogin(LoginRequest request);

}
