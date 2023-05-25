using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Responses;
using FMCW.Common.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IUserService : IBaseService
{
    public Task<DTOResult<LoginResponse>> TryLogin(LoginRequest request);
}
