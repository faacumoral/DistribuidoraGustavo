using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Services;
using DistribuidoraGustavo.Interfaces.Settings;
using FMCW.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services;

public class UserService : IUserService
{
    private readonly DistribuidoraGustavoContext _context;
    
    public UserService(DistribuidoraGustavoContext context)
    {
        _context = context;
    }

    public async Task<DTOResult<UserModel>> TryLogin(LoginRequest request)
    {
        var encryptPassword = ApplicationSettings.Config.EncryptKey;

        var encodedPassword = FMCW.Seguridad.Encriptador.Encriptar(request.Password, encryptPassword);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return DTOResult<UserModel>.Error("Usuario y/o password incorrecto");
        }

        return DTOResult<UserModel>.Ok(new UserModel
        {
            
        });
    }


}

