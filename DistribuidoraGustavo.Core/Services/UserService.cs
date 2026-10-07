using DistribuidoraGustavo.Core.Security;
using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Responses;
using DistribuidoraGustavo.Interfaces.Services;
using DistribuidoraGustavo.Interfaces.Settings;
using DistribuidoraGustavo.Interfaces.Results;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services;

public class UserService : BaseService, IUserService
{
    private readonly DistribuidoraGustavoContext _context;
    private readonly IJwtManager _jwtManager;

    public UserService(
        DistribuidoraGustavoContext context,
        IJwtManager jwtManager)
    {
        _context = context;
        _jwtManager = jwtManager;
    }

    public async Task<DTOResult<LoginResponse>> TryLogin(LoginRequest request)
    {
        var encryptPassword = ApplicationSettings.Config.EncryptKey;

        var encodedPassword = PasswordEncryptor.Encrypt(request.Password, encryptPassword);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == encodedPassword);

        if (user == null)
            return DTOResult<LoginResponse>.Error("Usuario y/o password incorrecto");

        if (user.Active != true)
            return DTOResult<LoginResponse>.Error("El usuario está deshabilitado");

        var jwt = _jwtManager.GenerateToken(user.UserId);

        return DTOResult<LoginResponse>.Ok(new LoginResponse
        {
            Jwt = jwt,
            User = user.ToModel()
        });
    }


}

